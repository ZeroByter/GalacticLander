using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
    public static LevelLoader Singletron;
    public static string LastLevelName;
    public static float CurrentLevelSeed;

    private static bool _GravityEnabled = true;
    public static bool GravityEnabled {
        get {
            if (NetworkingManager.CurrentLobbyValid) { //if we are in a lobby
                CSteamID ownerId = SteamMatchmaking.GetLobbyOwner((CSteamID)NetworkingManager.CurrentLobby);
                return SteamMatchmaking.GetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, ownerId, "voteGravity") == "True";
            } else {
                return _GravityEnabled;
            }
        }
        set {
            if (NetworkingManager.CurrentLobbyValid) { //if we are in a lobby
                SteamMatchmaking.SetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, "voteGravity", value.ToString());
                _GravityEnabled = true;
            } else {
                _GravityEnabled = value;
            }
        }
    }

    public Transform backgroundMist;

    [HideInInspector]
    public LevelData levelData;

    private GameObject filledTile;
    private GameObject filledTileSpaceFighters;

    public enum LevelOrigin { Game, External };
    /// <summary>
    /// Is the level from the game or created/downloaded from the workshop
    /// </summary>
    private static LevelOrigin GameLevelOrigin;
    private static string GameLevelDirectory; //in case of game-level, cast to int
    /// <summary>
    /// If set to true, then level progress won't be saved and played will be sent back to level editor scene with the level data object
    /// </summary>
    public static bool PlayTestingLevel;

    public static void SetLevelDirectory(LevelOrigin origin, string levelDirectory, bool playTestingLevel = false) {
        LastLevelName = GameLevelDirectory;

        GameLevelOrigin = origin;
        GameLevelDirectory = levelDirectory;
        PlayTestingLevel = playTestingLevel;
    }

    public static bool IsLevelDirectoryValid() {
        return !string.IsNullOrEmpty(GameLevelDirectory);
    }

    public static string GetLevelDirectory() {
        return GameLevelDirectory;
    }

    public static bool IsPlayingSharedScreenCoop(bool checkForPlaytesting) {
        if (Singletron == null) return false;
        if (!Singletron.levelData.IsCoopLevel()) return false;
        if (NetworkingManager.CurrentLobbyValid) return false;
        if (checkForPlaytesting) {
            if (PlayTestingLevel) return false;
        }

        return true;
    }

    public static bool IsPlayingEventLevel() {
        GameEvent currentEvent = GameEvents.GetCurrentEvent();
        if (Singletron != null && currentEvent != null) { //if the level loader exists and there is an event currently running
            //we check if we are playing an event level
            if (GetLevelDirectory().StartsWith(currentEvent.name + "/" + currentEvent.name + "_")) { //if we are playing an event level...
                return true;
            }
        }

        return false;
    }

    private Transform levelTilesParent;
    private Transform fillInTilesParent;

    /// <summary>
    /// returns version comparison.
    /// 1 = game version is newer than comparison version
    /// 0 = same version
    /// -1 = gmae version is older than comparison version
    /// </summary>
    /// <param name="compareVersionStr"></param>
    /// <returns></returns>
    private int CompareVersions(string compareVersionStr) {
        System.Version currentVersion = new System.Version(Constants.Version);
        System.Version compareVersion = new System.Version(compareVersionStr);

        return currentVersion.CompareTo(compareVersion);
    }

    private void FillOutsideVoid(LevelData levelData, Color tilesColor, GameEvent currentEvent) {
        if (levelData.GetTiles().Count < 8) return;

        int edgeOffset = 15;
        Bounds expandedBounds = levelData.GetBounds();
        expandedBounds.Expand(edgeOffset * 2);
        Vector2 corner1 = expandedBounds.min;
        Vector2 corner2 = expandedBounds.max;

        for (float x = Mathf.Min(corner1.x, corner2.x); x <= Mathf.Max(corner1.x, corner2.x); x += 1) {
            for (float y = Mathf.Min(corner1.y, corner2.y); y <= Mathf.Max(corner1.y, corner2.y); y += 1) {
                Vector2 point = new Vector2(x, y);
                if (!levelData.IsPointInLevel(point) && !levelData.GetTiles().Contains(point)) {
                    GameObject newFilledTile = Instantiate(filledTile, point, Quaternion.identity, fillInTilesParent);

                    if(currentEvent == null)
                    {
                        newFilledTile.GetComponent<SpriteRenderer>().color = tilesColor;
                    }
                    else
                    {
                        newFilledTile.GetComponent<SpriteRenderer>().color = currentEvent.GetTileColor(x, y);
                    }

                    if (Random.Range(0,10000) == 0) {
                        newFilledTile.AddComponent<SpaceFightersFilledTileController>();
                        Instantiate(filledTileSpaceFighters, point, Quaternion.identity);
                    }
                }
            }
        }
    }

    public static float DarkerTileColor(float tileColor) {
        tileColor *= 255; //convert 0-1 float number to 0-255 number
        tileColor -= 15; //make it a bit darker
        tileColor /= 255; //convert it back to 0-1 float number
        return Mathf.Max(tileColor, 0);
    }

    public static Color DarkerTileColor(Color tileColor) {
        return new Color(DarkerTileColor(tileColor.r), DarkerTileColor(tileColor.g), DarkerTileColor(tileColor.b));
    }

    private void Awake() {
        if (string.IsNullOrEmpty(GameLevelDirectory)) {
            SceneManager.LoadScene("Main Menu");
            return;
        }

        levelTilesParent = new GameObject("Level Tiles Parent").transform;
        fillInTilesParent = new GameObject("Fillin Tiles Parent").transform;

        CursorController.RemoveAllUsers();

        CurrentLevelSeed = Random.Range(0, 90000);

        Singletron = this;

        filledTile = (GameObject) Resources.Load("Collision Data/Filled Tile", typeof(GameObject));
        filledTileSpaceFighters = (GameObject) Resources.Load("Collision Data/Filled Tile Space Fighters", typeof(GameObject));

        byte[] dataBytes = new byte[0];
        LevelData levelData;

        //setting gravity
        if (GravityEnabled) {
            Physics2D.gravity = new Vector2(0, -0.8f);
        } else {
            Physics2D.gravity = new Vector2(0, 0);
        }

        //resetting static 'next ids' variables and other static variables
        LandingPadController.ResetCapturedLandingPads();
        LandingPadController.NextLandPadId = 0;
        CrateController.NextCrateId = 0;

        //reading level
        if (GameLevelOrigin == LevelOrigin.Game) {
            Regex regex = new Regex(@"(sp|mp)(\d*)");

            Match match = regex.Match(GameLevelDirectory);

            //setting whether the player can play the next level after this one
            if (match.Groups[1].Value == "mp" || match.Groups[1].Value == "sp") {
                VictoryMenuController.Singletron.showNextLevelButton = true;
                VictoryMenuController.Singletron.currentLevelNumber = int.Parse(match.Groups[2].Value);
            }

            TextAsset rawData = Resources.Load<TextAsset>(GameLevelDirectory);
            if(rawData == null) {
                PromptsController.OpenErrorPrompt("Missing from-game level file! (name = " + GameLevelDirectory + ")");
                SceneManager.LoadScene("Main Menu");
                return;
            } else {
                dataBytes = rawData.bytes;
            }
        } else if(GameLevelOrigin == LevelOrigin.External) {
            VictoryMenuController.Singletron.showNextLevelButton = false;
            FileStream file = File.Open(GameLevelDirectory, FileMode.Open);
            file.Position = 0;
            dataBytes = new byte[file.Length];
            for(int i = 0; i < file.Length; i++) {
                dataBytes[i] = (byte)file.ReadByte();
            }
            file.Close();
        }

        //unpacking level
        BinaryFormatter bf = new BinaryFormatter();
        using (Stream stream = new MemoryStream(dataBytes)) {
            levelData = (LevelData)bf.Deserialize(stream);
        }

        if (levelData != null) {
            this.levelData = levelData;
            List<Vector2> tiles = new List<Vector2>();
            int visualLogicNumber = 1;

            Color tilesColor;
            if (levelData.useCustomTilesColor) {
                tilesColor = levelData.tilesColor;

                Vector3 mistScale = backgroundMist.localScale;
                mistScale.y = levelData.backgroundMistSize;
                backgroundMist.localScale = mistScale;
            } else {
                tilesColor = new Color32(45, 45, 45, 255);

                Vector3 mistScale = backgroundMist.localScale;
                mistScale.y = 1.5f;
                backgroundMist.localScale = mistScale;
            }

            GameEvent currentEvent = GameEvents.GetCurrentEvent();
            if(IsPlayingEventLevel()) {
                if(GameLevelDirectory.StartsWith(currentEvent.name + "/" + currentEvent.name + "_")) {
                    tilesColor = currentEvent.themeColor;

                    Vector3 mistScale = backgroundMist.localScale;
                    mistScale.y = 1.5f;
                    backgroundMist.localScale = mistScale;
                }
                else
                {
                    currentEvent = null;
                }
            }
            else
            {
                currentEvent = null;
            }

            //adjusting background tiles color
            Shader.SetGlobalColor("_ReplaceColor", new Color(DarkerTileColor(tilesColor.r), DarkerTileColor(tilesColor.g), DarkerTileColor(tilesColor.b), 1));

            //creating tiles
            foreach (LevelObject obj in levelData.levelData) {
                if(obj.GetType() == typeof(LevelTile)) {
                    LevelTile tile = (LevelTile)obj;
                    GameObject collisionPrefab = (GameObject) Resources.Load("Collision Data/" + tile.spriteName, typeof(GameObject));
                    GameObject newTile = Instantiate(collisionPrefab, new Vector2(tile.x, tile.y), Quaternion.Euler(0,0,tile.rotation));
                    newTile.transform.localScale = new Vector3(tile.scaleX, tile.scaleY);
                    newTile.layer = 9;

                    LevelObjectHolder objectHolder = newTile.GetComponent<LevelObjectHolder>();
                    if (objectHolder != null) objectHolder.levelObject = tile;
                    
                    if(currentEvent == null)
                    {
                        newTile.GetComponent<SpriteRenderer>().color = tilesColor;
                    }
                    else
                    {
                        newTile.GetComponent<SpriteRenderer>().color = currentEvent.GetTileColor(tile.x, tile.y);
                    }

                    tiles.Add(newTile.transform.position);

                    newTile.transform.parent = levelTilesParent;
                }

                if(obj.GetType() == typeof(LevelEntity) || obj.isEntity) {
                    LevelEntity entity = (LevelEntity)obj;
                    GameObject prefab;
                    if (entity.resourceName == "" && entity.randomResourceNames.Length > 0) {
                        prefab = (GameObject)Resources.Load(entity.randomResourceNames[Random.Range(0, entity.randomResourceNames.Length)], typeof(GameObject));
                    } else {
                        prefab = (GameObject)Resources.Load(entity.resourceName, typeof(GameObject));
                    }
                    GameObject newEntity = Instantiate(prefab, new Vector2(entity.x, entity.y), Quaternion.Euler(0, 0, entity.rotation));

                    SpriteRenderer spriteRenderer = newEntity.GetComponent<SpriteRenderer>();
                    if(spriteRenderer != null) spriteRenderer.sortingOrder = -1;

                    if (prefab.name.StartsWith("Spike")) {
                        spriteRenderer.color = tilesColor;

                        newEntity.layer = 9;
                    } else {
                        newEntity.layer = 11;
                    }

                    //logic entity
                    if (entity.isLogicEntity && entity.isLogicActivator) {
                        entity.logicNumber = visualLogicNumber;
                        visualLogicNumber++;
                    }

                    //handle object holder component
                    LevelObjectHolder objectHolder = newEntity.GetComponent<LevelObjectHolder>();
                    if(objectHolder == null) { //if the component doesn't exist on the prefab
                        objectHolder = newEntity.AddComponent<LevelObjectHolder>(); //create the component
                        objectHolder.levelEntity = entity; //assign the entity
                    } else {
                        objectHolder.levelEntity = entity; //assign the entity to the already existing component
                    }
                }
            }

            FillOutsideVoid(levelData, tilesColor, currentEvent);

            if(levelData.workshopId != 0) { //if this is a workshop level
                SteamUGC.StartPlaytimeTracking(new PublishedFileId_t[] { new PublishedFileId_t(levelData.workshopId) }, 1);
            }
        }
    }

    public int GetCurrentEventLevelNumber() {
        //if we are playing a game event level, here we check which level # we are on
        if (IsPlayingEventLevel()) { //if are we playing a game event level?
            GameEvent currentEvent = GameEvents.GetCurrentEvent();

            string spReplace = string.Format("{0}/{0}_sp", currentEvent.name);
            string mpReplace = string.Format("{0}/{0}_mp", currentEvent.name);

            return int.Parse(GameLevelDirectory.Replace(spReplace, "").Replace(mpReplace, ""));
        } else {
            return 0;
        }
    }

    private void Start() {
        foreach(LandingPadController landingPad in LandingPadController.AllPads) {
            landingPad.isCapturing = false;
        }
    }

    public void RestartLevel() {
        if(SteamManager.Initialized && NetworkingManager.CurrentLobbyValid) { //if we are in a lobby/coop
            CoopVoteController.StartVote(CoopVoteController.VoteType.RestartLevel);
        } else {
            if (IsPlayingEventLevel() && VictoryMenuController.LevelCompleted) {
                GameEvents.CurrentEventProgression--;
            }

            SceneManager.LoadScene("Game Level");
        }
    }
}
