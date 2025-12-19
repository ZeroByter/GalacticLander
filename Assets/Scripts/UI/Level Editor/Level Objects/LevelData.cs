using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class VectorHolder {
    public Vector2 vector;

    public VectorHolder() { }
    public VectorHolder(Vector2 newVector) {
        vector = newVector;
    }

    public override string ToString() {
        return vector.ToString();
    }
}

class Line {
    public Vector2 point1;

    public Vector2 point2;
    public float length {
        get {
            return Vector2.Distance(point1, point2);
        }
    }

    public Line() { }

    public Line(Vector2 p1, Vector2 p2) {
        point1 = p1;
        point2 = p2;
    }

    public override string ToString() {
        return "Line(from " + point1 + " to " + point2 + ")";
    }
}

[Serializable]
public class LevelData {
    public string gameVersion; //the game version this level was created with
    public string steamId; //the id of this workshop creation
    /// <summary>
    /// This is used cause steamId was actually supposed to be a ulong but in a lot of levels it's a string so gotta add workshopId
    /// </summary>
    public ulong workshopId {
        get {
            ulong result = 0;
            if (ulong.TryParse(steamId, out result)) {
                return result;
            } else {
                return 0;
            }
        }
        set {
            steamId = value.ToString();
        }
    }
    /// <summary>
    /// Deprecated, we can just get this from `GetQueryUGCResult`
    /// </summary>
    public ulong creator = 0; //steamid of creator, if equals to developer steamid then display in-game as 'zerobyter games'
    public DateTime timeCreated;
    public DateTime lastModified;

    /// <summary>
    /// Used for backwards compability
    /// </summary>
    public bool useCustomTilesColor;
    public float tilesColorR = 45 / 255;
    public float tilesColorG = 45 / 255;
    public float tilesColorB = 45 / 255;
    public Color tilesColor {
        get {
            float maxColor = 0.2352941176470588f;
            return new Color(Mathf.Min(maxColor, tilesColorR), Mathf.Min(maxColor, tilesColorG), Mathf.Min(maxColor, tilesColorB));
        }
        set {
            tilesColorR = value.r;
            tilesColorG = value.g;
            tilesColorB = value.b;
        }
    }
    public float backgroundMistSize = 2.45f;

    public List<LevelObject> levelData = new List<LevelObject>();

    public float[] levelMapValues = new float[0];

    [NonSerialized]
    private List<Vector2> tiles = new List<Vector2>();
    [NonSerialized]
    public List<LevelTile> levelTiles = new List<LevelTile>();
    [NonSerialized]
    private List<Line> lines = new List<Line>();

    [NonSerialized]
    private bool boundsValid = false; //gets set to false if we changed any tiles, true if we refreshed the bounds
    [NonSerialized]
    private Bounds levelBounds = new Bounds();

    public LevelData() {
        gameVersion = Constants.Version;
        if (creator == 0 && SteamManager.DoesHaveInstance && SteamManager.Initialized) creator = (ulong)SteamUser.GetSteamID();
        timeCreated = DateTime.Now;
        lastModified = DateTime.Now;

        levelData.Add(new LevelEntity("Ship Pads/Launch Pad") { canAdvancedModify = false, y = 0.2f, spriteName = "Landing Pads/Launch Pad Static Sprite" });
        levelData.Add(new LevelEntity("Ship Pads/Land Pad") { canAdvancedModify = false, x = 4, y = 0.2f, spriteName = "Landing Pads/Land Pad Static Sprite" });
    }

    public static LevelData LoadLevelData(string levelName) {
        byte[] dataBytes = new byte[0];
        LevelData levelData;

        //reading level
        if (levelName.StartsWith("sp") || levelName.StartsWith("mp")) { //is the level a game level
            TextAsset rawData = (TextAsset)Resources.Load(levelName);
            dataBytes = rawData.bytes;
        }

        //unpacking level
        BinaryFormatter bf = new BinaryFormatter();
        using (Stream stream = new MemoryStream(dataBytes)) {
            levelData = (LevelData)bf.Deserialize(stream);
        }

        return levelData;
    }

    public List<LevelEntity> GetLogicSources(LevelEntity target) {
        List<LevelEntity> sources = new List<LevelEntity>();

        foreach (LevelObject obj in levelData) {
            if (obj.GetType() == typeof(LevelEntity)) {
                LevelEntity entity = (LevelEntity)obj;
                if (entity.isLogicEntity && entity.isLogicActivator && entity.logicTarget == target) {
                    sources.Add(entity);
                }
            }
        }

        return sources;
    }

    public LevelEntity GetLaunchPad() {
        foreach(LevelObject levelObject in levelData) {
            if(levelObject.GetType() == typeof(LevelEntity)) {
                LevelEntity entity = (LevelEntity)levelObject;

                if (entity.resourceName == "Ship Pads/Launch Pad") {
                    return entity;
                }
            }
        }

        return null;
    }

    public LevelEntity GetLandPad() {
        foreach (LevelObject levelObject in levelData) {
            if (levelObject.GetType() == typeof(LevelEntity)) {
                LevelEntity entity = (LevelEntity)levelObject;

                if (entity.resourceName == "Ship Pads/Land Pad") {
                    return entity;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Given a logic target entity, returns a flow from 0 to 1 that is a fraction of how much logic sources that are connected to the logic target are activated
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public float GetPercentOfActivatedSources(LevelEntity target) {
        List<LevelEntity> sources = GetLogicSources(target);
        float activatedSources = 0;

        foreach (LevelEntity entity in sources) {
            if (entity.isLogicActivated) activatedSources++;
        }

        return activatedSources / sources.Count;
    }

    public void InvalidateBounds() {
        boundsValid = false;
    }

    public Bounds GetBounds() {
        if (levelBounds.size == Vector3.zero || !boundsValid) {
            if(levelMapValues != null)
            {
                levelBounds = new Bounds(Vector3.zero, Vector3.one);
                for(float y = 0; y < 36; y += 0.5f)
                {
                    for(float x = 0; x < 36; x += 0.5f)
                    {
                        var point = new Vector3(x, y);
                        if(!IsPointInLevelNew(new Vector2(x, y)))
                        {
                            levelBounds.Encapsulate(point - new Vector3(18, 18));
                        }
                    }
                }
            }
            else
            {
                levelBounds = new Bounds(GetTiles()[0], Vector2.one * 2);
                foreach (Vector2 tile in GetTiles())
                {
                    levelBounds.Encapsulate(tile);
                }
            }
            boundsValid = true;
        }

        return levelBounds;
    }

    /// <summary>
    /// returns whether or not the level is a coop based on how many launcher pads are in the level data
    /// </summary>
    public bool IsCoopLevel() {
        int launchPadsCount = 0;
        int landPadsCount = 0;

        foreach (LevelObject obj in levelData) {
            if (obj.GetType() == typeof(LevelEntity)) {
                LevelEntity entity = (LevelEntity)obj;

                if (entity.resourceName == "Ship Pads/Launch Pad") launchPadsCount++;
                if (entity.resourceName == "Ship Pads/Land Pad") landPadsCount++;
            }
        }

        return launchPadsCount == 2 && landPadsCount == 2;
    }

    public void ListAllEntities() {
        Debug.Log("List of all map entities:");
        foreach (LevelObject obj in levelData) {
            if (obj.GetType() == typeof(LevelEntity)) {
                LevelEntity entity = (LevelEntity)obj;

                Debug.Log(string.Format("entity {0} is at position {1} with gameObject {2}", entity, entity.GetPosition(), entity.gameObject));
            }
        }
    }

    public LevelObject GetObjectAtPosition(Vector2 pos, bool exact = false) {
        foreach (LevelObject levelObject in levelData) {
            if (exact) {
                if (levelObject.GetPosition().x == pos.x && levelObject.GetPosition().y == pos.y) return levelObject;
            } else {
                if (Vector2.Distance(levelObject.GetPosition(), pos) < 0.1) return levelObject;
            }
        }

        return null;
    }

    public LevelObject GetEntityAtArea(Vector2 pos, float radius)
    {
        foreach (LevelObject levelObject in levelData)
        {
            if (levelObject is LevelEntity && Vector2.Distance(levelObject.GetPosition(), pos) < radius) return levelObject;
        }

        return null;
    }

    public string GetNameOfTileAtPosition(Vector2 pos) {
        foreach (LevelObject levelObject in levelData) {
            if (levelObject.GetType() == typeof(LevelTile) && levelObject.GetPosition().x == pos.x && levelObject.GetPosition().y == pos.y) return levelObject.spriteName;
        }

        return "";
    }

    public LevelTile GetTileAtPosition(Vector2 pos) {
        foreach (LevelObject levelObject in levelData) {
            if (levelObject.GetType() == typeof(LevelTile) && levelObject.GetPosition() == pos) return (LevelTile)levelObject;
        }

        return null;
    }

    /// <summary>
    /// returns the average position of all tiles in the level. returns negative infinity if error occured
    /// </summary>
    /// <returns></returns>
    public Vector2 GetAveragePosition() {
        List<Vector2> tiles = GetTiles();

        if (tiles.Count() < 8) return Vector2.negativeInfinity;

        float x = 0;
        float y = 0;

        foreach (Vector2 tile in tiles) {
            x += tile.x;
            y += tile.y;
        }

        return new Vector2(x / tiles.Count(), y / tiles.Count());
    }

    /// <summary>
    /// returns the farthest tile from the provided point. returns negative infinity if error
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector2 GetFarthestTileFromPosition(Vector2 pos) {
        List<Vector2> tiles = GetTiles();

        if (tiles.Count() < 8) return Vector2.negativeInfinity;

        Vector2 farthest = Vector2.zero;
        bool isValid = false;

        foreach (Vector2 tile in tiles) {
            if (!isValid || Vector2.Distance(tile, pos) > Vector2.Distance(farthest, pos)) {
                isValid = true;
                farthest = tile;
            }
        }

        return isValid ? farthest : Vector2.negativeInfinity;
    }

    public void SortTilesList() {
        if (tiles == null) tiles = new List<Vector2>();
        if (levelTiles == null) levelTiles = new List<LevelTile>();
        tiles.Clear();
        levelTiles.Clear();

        foreach (LevelObject obj in levelData) {
            if (obj.GetType() == typeof(LevelTile)) {
                tiles.Add(obj.GetPosition());
                levelTiles.Add((LevelTile)obj);
            }
        }
    }

    public List<Vector2> GetTiles() {
        if (tiles == null) tiles = new List<Vector2>();
        if (tiles.Count == 0) SortTilesList();

        return tiles;
    }

    public List<LevelTile> GetTilesByFamilyName(params string[] families) {
        SortTilesList();
        List<LevelTile> returnList = new List<LevelTile>();

        foreach (LevelTile tile in levelTiles) {
            bool addTile = false;

            foreach (string familyName in families) {
                if (tile.spriteName.StartsWith(familyName)) {
                    addTile = true;
                    break;
                }
            }

            if (addTile) returnList.Add(tile);
        }

        return returnList;
    }

    public bool IsLevelInclosed() {
        if (GetTiles().Count < 8) return false; //if the level has less than 8 tiles it can not possibly be closed, therefore it is false

        Bounds levelBounds = GetBounds(); //get all bounds
        List<Vector2> tiles = GetTiles(); //get all tiles

        for (float x = levelBounds.min.x; x < levelBounds.max.x; x += 1) { //we loop through each gridbox inside the level bounds
            for (float y = levelBounds.min.y; y < levelBounds.max.y; y += 1) {
                Vector2 vector = new Vector2(x, y);

                if (tiles.Contains(vector)) continue; //if this point is a tile, we skip the loop

                if (IsPointInLevel(vector)) { //if this point is inside the level
                    Vector2 up = vector + Vector2.up; //we define all the neighbours
                    Vector2 down = vector + Vector2.down;
                    Vector2 left = vector + Vector2.left;
                    Vector2 right = vector + Vector2.right;

                    bool isUpInLevel = IsPointInLevel(up); //we check if every neighbour is inside the level
                    bool isDownInLevel = IsPointInLevel(down);
                    bool isLeftInLevel = IsPointInLevel(left);
                    bool isRightInLevel = IsPointInLevel(right);

                    if (!isUpInLevel || !isDownInLevel || !isLeftInLevel || !isRightInLevel) { //if one of the neighbours isn't inside the level, we return false
                        return false;
                    }
                }
            }
        }

        return true; //if everything went well, we return true
    }

    /// <summary>
    /// given a tile in the level, this returns the top most tile above it
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    private Vector2 GetTopMostTile(Vector2 tile) {
        Vector2 topPosition = new Vector2(tile.x, tile.y + 1f);

        if (GetTiles().Contains(topPosition)) {
            return GetTopMostTile(topPosition);
        } else {
            return tile;
        }
    }

    /// <summary>
    /// given a tile in the level, this returns the bottom most tile underneath it
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    private Vector2 GetBottomMostTile(Vector2 tile) {
        Vector2 bottomPosition = new Vector2(tile.x, tile.y - 1f);

        if (GetTiles().Contains(bottomPosition)) {
            return GetBottomMostTile(bottomPosition);
        } else {
            return tile;
        }
    }

    #region Get all lines to the right
    private Line GetLineToDirection(Line currentLine, int direction) {
        foreach(Line line in lines) {
            if (line.point1.y == currentLine.point1.y && line.point2.y == currentLine.point2.y && line.point1.x == currentLine.point1.x + direction) return line;
        }

        return null;
    }

    private Line GetLeftMostLine(Line currentLine) {
        Line lineToLeft = GetLineToDirection(currentLine, -1);

        if(lineToLeft != null) {
            return GetLeftMostLine(lineToLeft);
        } else {
            return currentLine;
        }
    }

    private List<Line> _GetAllLinesToTheRight(Line currentLine, List<Line> list) {
        Line lineToTheRight = GetLineToDirection(currentLine, 1);

        if(lineToTheRight != null) { //if there is a line to the right
            list.Add(lineToTheRight); //add it to the list
            return _GetAllLinesToTheRight(lineToTheRight, list); //continue recursive
        } else {
            return list; //return the (now full) list
        }
    }

    /// <summary>
    /// Returns a list of all lines to the right of this line where both points share the same y
    /// </summary>
    /// <param name="leftMostLine"></param>
    /// <returns></returns>
    private List<Line> GetAllLinesToRight(Line leftMostLine) {
        List<Line> allLinesToTheRight = new List<Line>();

        return _GetAllLinesToTheRight(leftMostLine, allLinesToTheRight);
    }

    private void RemoveAllLines(List<Line> linesToRemove) {
        for (int i = 0; i < linesToRemove.Count; i++) {
            Line lineToRemove = linesToRemove[i];

            lines.Remove(lineToRemove);
        }
    }
    #endregion

    public void SortLines() {
        if (lines == null) lines = new List<Line>();
        lines.Clear();

        GetTiles(); //we call this here to make sure the list is properly populated

        foreach (LevelTile tileObj in levelTiles) {
            Vector2 tile = tileObj.GetPosition();

            Vector2 topMostTile = GetTopMostTile(tile);
            Vector2 bottomMostTile = GetBottomMostTile(tile);

            Line verticalLine = new Line(topMostTile, bottomMostTile);
            if (tile != bottomMostTile) {
                Line otherLine = lines.Find(u => u.point2 == bottomMostTile);
                if (otherLine == null) {
                    lines.Add(verticalLine);
                }
            }
        }
        
        for(int i = 0; i < lines.Count; i++) {
            Line line = lines[i];

            Line leftMostLine = GetLeftMostLine(line);
            List<Line> linesToTheRight = GetAllLinesToRight(leftMostLine);

            if(linesToTheRight.Count > 1) { //if there are more than two lines to the right
                linesToTheRight.RemoveAt(linesToTheRight.Count - 1);

                RemoveAllLines(linesToTheRight);
            }
        }

        lines.Distinct();
    }

    private List<Line> GetLines() {
        if (lines == null) lines = new List<Line>();
        if (lines.Count == 0) SortLines();

        return lines;
    }

    private bool LineIntersectsLine(Vector2 l1p1, Vector2 l1p2, Vector2 l2p1, Vector2 l2p2) {
        float q = (l1p1.y - l2p1.y) * (l2p2.x - l2p1.x) - (l1p1.x - l2p1.x) * (l2p2.y - l2p1.y);
        float d = (l1p2.x - l1p1.x) * (l2p2.y - l2p1.y) - (l1p2.y - l1p1.y) * (l2p2.x - l2p1.x);
        if (d == 0) return false;
        float r = q / d;
        q = (l1p1.y - l2p1.y) * (l1p2.x - l1p1.x) - (l1p1.x - l2p1.x) * (l1p2.y - l1p1.y);
        float s = q / d;
        if (r < 0 || r > 1 || s < 0 || s > 1) return false;
        return true;
    }

    public bool IsPointInLevel(Vector2 point, bool includeTiles = true/*, bool twoIntersects = true*/) {
        List<Vector2> tiles = GetTiles();
        if (tiles.Count < 8) return false;

        if (includeTiles) {
            if (tiles.Contains(point)) return true;
        } else {
            if (tiles.Contains(point)) return false;
        }

        Vector2 corner1 = GetBounds().min;
        Vector2 corner2 = GetBounds().max;

        if (point.x <= Mathf.Min(corner1.x, corner2.x) || point.y >= Mathf.Max(corner1.y, corner2.y)) return false;
        if (point.x >= Mathf.Max(corner1.x, corner2.x) || point.y <= Mathf.Min(corner1.y, corner2.y)) return false;

        int lineHits = 0;

        /*Line[] linesArray = GetLines().ToArray();
        Array.Sort(linesArray, (l1, l2) => Mathf.CeilToInt(l1.point1.x).CompareTo(Mathf.CeilToInt(l2.point1.x))); //sort lines
        List<Line> lines = linesArray.ToList();*/
        Line[] lines = GetLines().ToArray();

        foreach (Line line in lines) {
            //Debug.DrawLine(line.point1, line.point2, Color.red, 2);

            if (LineIntersectsLine(line.point1 - new Vector2(0, 0.5f), line.point2, new Vector2(point.x - 500, point.y), point)) {
                lineHits++;
            }
        }

        return lineHits % 2 != 0;
    }

    public bool IsPointInLevelNew(Vector2 point)
    {
        var newX = Mathf.RoundToInt(point.x / 36f * 150f);
        var newY = Mathf.RoundToInt(point.y / 36f * 150f);
        var index = newX + newY * MarchingSquaresManager.DataWidth;

        if (index < 0 || index > levelMapValues.Length) return false;

        return levelMapValues[index] >= MarchingSquaresManager.IsoLevel;
    }
}
