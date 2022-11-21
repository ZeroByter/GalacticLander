using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class LevelEditorManager : MonoBehaviour {
    public static LevelEditorManager Singletron;

    public static bool LaunchTutorial;

    [Header("The toggle coop button")]
    public TMP_Text toggleCoopText;

    [Header("The working level editor table")]
    public Transform workingTable;
    public GameObject tempOutsideFilledTile;

    [Header("Theme prompt controller")]
    public LevelEditorThemePromptController themePromptController;
    
    [Header("The load editor prompt menu")]
    public CanvasBlurTransition loadEditorMenu;

    [Header("Tile template")]
    public SpriteRenderer template;

    [HideInInspector]
    public LevelData levelData;

    /// <summary>
    /// Used for when we spawn in a coop ship launch pad
    /// </summary>
    private LevelEntity coopLaunchPadEntity = new LevelEntity("Ship Pads/Launch Pad") { x = 0, y = 2, canAdvancedModify = false, canOverrideDelete = true, spriteName = "Landing Pads/Launch Pad Static Sprite" };
    /// <summary>
    /// Used for when we spawn in a coop ship land pad
    /// </summary>
    private LevelEntity coopLandingPadEntity = new LevelEntity("Ship Pads/Land Pad") { x = 4, y = 2, canAdvancedModify = false, canOverrideDelete = true, spriteName = "Landing Pads/Land Pad Static Sprite" };

    private float targetSize = 0.5f;
    private Vector2 draggingStartLevelPosition;
    private Vector2 draggingStartMousePosition;
    private float minSize = 2;
    private float maxSize = 8;

    private Camera mainCamera;

    private void Awake() {
        if(Time.time == 0f) {
            SceneManager.LoadScene("Main Menu");
            return;
        }

        Singletron = this;

        levelData = new LevelData();

        CursorController.AddUser("mainMenu");

        mainCamera = Camera.main;

        MarchingSquaresManager.CreateBlank();
        MarchingSquaresManager.GenerateMesh();

        levelData.levelMapValues = MarchingSquaresManager.GetValues();

        //awake is called as soon as the level editor scene is loaded
        if (LevelLoader.PlayTestingLevel) { //so if a level is being playtested, we can check so here
            FileStream levelFile = File.Open(Application.persistentDataPath + "/Level Editor/tempLevel.level", FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            SetLevelData((LevelData)bf.Deserialize(levelFile));
            levelFile.Close();

            LevelLoader.PlayTestingLevel = false;
        }
    }

    public void BeginLevelEditorTutorial()
    {
        LaunchTutorial = true;
        SceneManager.LoadScene("Level Editor");
    }

    private void Start()
    {
        if (LaunchTutorial)
        {
            loadEditorMenu.ForceClose();
            LaunchTutorial = false;
            LevelEditorTutorialManager.StartTutorial();
        }
    }

    private void Update() {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetKeyDown(KeyCode.RightShift)) {
                int edgeOffset = 3;
                levelData.GetTiles();
                if(levelData.GetTiles().Count > 7) {
                    levelData.SortLines();

                    levelData.InvalidateBounds();
                    Bounds expandedBounds = levelData.GetBounds();
                    expandedBounds.Expand(edgeOffset * 2);
                    Vector2 corner1 = expandedBounds.min;
                    Vector2 corner2 = expandedBounds.max;

                    for (float x = Mathf.Min(corner1.x, corner2.x); x <= Mathf.Max(corner1.x, corner2.x); x += 1) {
                        for (float y = Mathf.Min(corner1.y, corner2.y); y <= Mathf.Max(corner1.y, corner2.y); y += 1) {
                            Vector2 point = new Vector2(x, y);

                            if (!levelData.IsPointInLevel(point)) {
                                GameObject newTempTile = Instantiate(tempOutsideFilledTile, point, Quaternion.identity);
                                newTempTile.SetActive(true);
                            }
                        }
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.RightShift)) {
                foreach(GameObject go in GameObject.FindGameObjectsWithTag("TempOutsideTile")) {
                    Destroy(go);
                }
            }

            Vector2 mousePos = MainCameraController.Singletron.selfCamera.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButtonDown(2)) {
                draggingStartLevelPosition = mainCamera.transform.position;
                draggingStartMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(2)) {
                mainCamera.transform.position = new Vector3(0, 0, -30) + (Vector3)(draggingStartLevelPosition + ((Vector2)mainCamera.ScreenToWorldPoint(draggingStartMousePosition) - mousePos));
            }

            int editorSize = 18; //the size of the editor working table

            float editorSizeWithLeftPanel = editorSize - (editorSize + Screen.width * 0.2312292f / (Screen.width / (MainCameraController.Singletron.selfCamera.orthographicSize * MainCameraController.Singletron.selfCamera.aspect)));
            editorSizeWithLeftPanel *= 2;

            if (mainCamera.transform.position.y + mainCamera.orthographicSize > editorSize) mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, editorSize - mainCamera.orthographicSize, -30); //clamp camera to top of editor
            if (mainCamera.transform.position.x + mainCamera.orthographicSize * mainCamera.aspect > editorSize) mainCamera.transform.position = new Vector3(editorSize - mainCamera.orthographicSize * mainCamera.aspect, mainCamera.transform.position.y, -30); //clamp camera to right of editor
            if (mainCamera.transform.position.x - mainCamera.orthographicSize * mainCamera.aspect < editorSizeWithLeftPanel - editorSize) mainCamera.transform.position = new Vector3(editorSizeWithLeftPanel - editorSize + mainCamera.orthographicSize * mainCamera.aspect, mainCamera.transform.position.y, -30); //clamp camera to left of editor
            if (mainCamera.transform.position.y - mainCamera.orthographicSize < -editorSize) mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, -editorSize + mainCamera.orthographicSize, -30); //clamp camera to bottom of editor

            //detecting when to zoom the map
            float mouseScrollwheel = Input.GetAxis("Mouse ScrollWheel");
            if (mouseScrollwheel > 0) {
                targetSize -= 0.1f;
            } else if (mouseScrollwheel < 0) {
                targetSize += 0.1f;
            }

            //standard boundary checks
            if (targetSize > 1) targetSize = 1;
            if (targetSize < 0) targetSize = 0;

            float orthoSize = Mathf.Lerp(minSize, maxSize, targetSize);
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, orthoSize, 0.65f);
            if (Mathf.Approximately(mainCamera.orthographicSize, orthoSize)) mainCamera.orthographicSize = orthoSize;
        }
    }

    public static LevelData GetLevelData() {
        if (Singletron == null) return null;
        return Singletron.levelData;
    }

    private static Sprite GetSpriteFromSprites(Sprite[] sprites, string target) {
        foreach(Sprite sprite in sprites) {
            if (sprite.name == target) return sprite;
        }
        return null;
    }

    public static void SetLevelData(LevelData newData) {
        if (Singletron == null) return;
        Singletron.levelData = newData;

        if (newData.useCustomTilesColor) {
            Singletron.themePromptController.Setup(newData.backgroundMistSize, newData.tilesColor);
        } else {
            Singletron.themePromptController.Setup(1.5f, new Color32(45, 45, 45, 255));
        }

        Singletron.toggleCoopText.text = newData.IsCoopLevel() ? "Change To Singleplayer" : "Change To Coop";

        //First we delete all old tiles/entities
        foreach (GameObject oldTile in GameObject.FindGameObjectsWithTag("Level")) {
            Destroy(oldTile);
        }
        foreach (GameObject oldEntity in GameObject.FindGameObjectsWithTag("Entity")) {
            Destroy(oldEntity);
        }

        Sprite[] tileSprites = Resources.LoadAll<Sprite>("Ground Sprites");
        Dictionary<LevelObject, GameObject> spawnedEntities = new Dictionary<LevelObject, GameObject>();
        Dictionary<Transform, LevelObject> logicEntitiesToConnect = new Dictionary<Transform, LevelObject>();

        LevelEditorLinesController.DestroyAllLines();

        if(newData.levelMapValues != null)
        {
            MarchingSquaresManager.SetData(newData.levelMapValues);
            MarchingSquaresManager.GenerateMesh();
        }
        else
        {
            MarchingSquaresManager.SetDataFromOldLevel(newData);
            MarchingSquaresManager.GenerateMesh();
        }

        //Go through all entities and tiles in new loaded `LevelData` and instantiate them inside the level editor
        foreach (LevelObject obj in newData.levelData) {
            /*if(obj.GetType() == typeof(LevelTile)) {
                LevelTile tile = (LevelTile)obj;

                SpriteRenderer newSprite = Instantiate(Singletron.template, Singletron.template.transform.parent);
                newSprite.sprite = GetSpriteFromSprites(tileSprites, tile.spriteName);
                newSprite.gameObject.layer = 9;
                newSprite.gameObject.AddComponent(typeof(BoxCollider2D));
                newSprite.transform.localPosition = tile.GetPosition();
                //newSprite.color = Color.white;
                newSprite.transform.eulerAngles = tile.GetQuaternion();
                newSprite.transform.localScale = tile.GetScale();
                newSprite.gameObject.tag = "Level";
                tile.gameObject = newSprite.gameObject;

                LevelObjectHolder objectHolder = newSprite.gameObject.AddComponent<LevelObjectHolder>();
                objectHolder.levelTile = tile;
            }*/

            if(obj.GetType() == typeof(LevelEntity) || obj.isEntity) {
                LevelEntity entity = (LevelEntity)obj;

                entity.isEntity = true;

                SpriteRenderer newSprite = Instantiate(Singletron.template, Singletron.template.transform.parent);
                newSprite.sprite = Resources.Load<Sprite>(entity.spriteName);
                newSprite.gameObject.layer = 11;
                newSprite.gameObject.tag = "Entity";
                newSprite.gameObject.AddComponent(typeof(BoxCollider2D));
                newSprite.transform.localPosition = entity.GetPosition();
                newSprite.color = Color.white;
                newSprite.transform.eulerAngles = entity.GetQuaternion();
                newSprite.transform.localScale = entity.GetScale();
                entity.gameObject = newSprite.gameObject;

                entity.ActivatedEditor(newSprite.gameObject);

                spawnedEntities.Add(entity, newSprite.gameObject);

                if (entity.isLogicEntity) {
                    if (entity.isLogicActivator) {
                        if (entity.logicTarget != null) {
                            if (spawnedEntities.ContainsKey(entity.logicTarget)) { //if we already spawned the logic target entity...
                                LevelEditorLinesController.CreateOrEditLine(newSprite.transform, spawnedEntities[entity.logicTarget].transform); //then we find the transform of the gameobject of the logic target we are trying to connect a graphical line with
                            } else {
                                logicEntitiesToConnect.Add(newSprite.transform, entity.logicTarget);
                            }
                        }
                    } else {
                        if (logicEntitiesToConnect.ContainsValue(entity)) {
                            foreach(KeyValuePair<Transform, LevelObject> pair in logicEntitiesToConnect) {
                                if(pair.Value == entity) {
                                    LevelEditorLinesController.CreateOrEditLine(pair.Key, newSprite.transform); //we create a graphical line from the transform of the gameobject of the source activator logic entity to this target logic entity
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private GameObject CreateCoopPadsSprites(LevelEntity shipPadEntity) {
        SpriteRenderer newPrefab = Instantiate(LevelEditorCursor.Singletron.template, LevelEditorCursor.Singletron.transform.parent);
        newPrefab.sprite = Resources.Load<Sprite>(shipPadEntity.spriteName);
        newPrefab.color = Color.white;
        newPrefab.gameObject.SetActive(true);
        newPrefab.gameObject.AddComponent(typeof(BoxCollider2D));
        newPrefab.gameObject.layer = 11; //layer 11 means entity
        newPrefab.gameObject.tag = "Entity";
        newPrefab.transform.position = new Vector2(shipPadEntity.x, shipPadEntity.y); //setting the 2d position

        return newPrefab.gameObject;
    }

    public void ToggleCoop() {
        bool isCoop = levelData.IsCoopLevel(); //toggle the bool

        if (isCoop) {
            toggleCoopText.text = "Change To Coop";

            //here we delete the two old coop-launch and landing pads level entities becuase we are switching the level from a coop level to a singleplayer level
            List<LevelObject> objectsToDelete = new List<LevelObject>();

            foreach(LevelObject levelObject in levelData.levelData){
                if (levelObject.spriteName.EndsWith("Launch Pad Static Sprite") || levelObject.spriteName.EndsWith("Land Pad Static Sprite")) {
                    if (levelObject.canOverrideDelete) {
                        Destroy(levelObject.gameObject); //destroy the actual physical gameobject
                        objectsToDelete.Add(levelObject);
                    }
                }
            }

            foreach(LevelObject @object in objectsToDelete) {
                levelData.levelData.Remove(@object);
            }
            objectsToDelete = null;
        } else {
            toggleCoopText.text = "Change To Singleplayer";

            GameObject launchpad = CreateCoopPadsSprites(coopLaunchPadEntity); //entity gameobject is set inside CreateCoopPadsSprites
            GameObject landPad = CreateCoopPadsSprites(coopLandingPadEntity); //entity gameobject is set inside CreateCoopPadsSprites

            LevelEntity newLaunchPad = (LevelEntity) coopLaunchPadEntity.GetDeepCopy();
            newLaunchPad.gameObject = launchpad;
            LevelEntity newLandPad = (LevelEntity) coopLandingPadEntity.GetDeepCopy();
            newLandPad.gameObject = landPad;

            levelData.levelData.Add(newLaunchPad);
            levelData.levelData.Add(newLandPad);
        }
    }

    public void PlaytestLevel() {
        string tempLevelPath = Application.persistentDataPath + "/Level Editor/tempLevel.level";
        FileStream file = File.Open(tempLevelPath, FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, GetLevelData());
        file.Close();

        LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.External, tempLevelPath, true);
        SceneManager.LoadScene("Game Level");
    }

    public void OpenConditionsGuideWebPage() {
        Steamworks.SteamFriends.ActivateGameOverlayToWebPage("https://galacticlander.zerobyter.net/workshop/conditions");
    }
}
