using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;

public class TileFamily {
    public string name;
    public int count;

    public static string SpriteNameToFamilyName(string spriteName) {
        Regex rx = new Regex(@" \d$");
        return rx.Replace(spriteName, "");
    }
}

public class LevelEditorCursor : MonoBehaviour {

    public static LevelEditorCursor Singletron;

    public SpriteRenderer template;
    public ButtonToggle eraserButtonToggle;
    public ButtonToggle tileRandomizerToggle;

    private LevelEntity movingEntityData;
    private GameObject prefab;
    [SerializeField]
    private LayerMask entityLayer;

    private LevelEntity firstEntityLogicLinking;

    private float rotation;
    private bool eraser;
    private float lastMouseClick;

    private bool placeTiles = true;

    private Sprite[] sprites;
    private List<TileFamily> tileFamilies = new List<TileFamily>();

    public static bool IsCurrentlyMovingObject()
    {
        if (Singletron == null) return false;

        return Singletron.movingEntityData != null;
    }

    private TileFamily GetTileFamily(string name) {
        foreach (TileFamily tileFamily in tileFamilies) {
            if (tileFamily.name == name) return tileFamily;
        }

        return null;
    }

    private Sprite GetRandomSpriteFromFamily(string familyName) {
        TileFamily family = GetTileFamily(familyName);

        int currentSpriteIndex = 0;

        for (int i = 0; i < 30; i++) {
            int newIndex = Random.Range(0, family.count - 1);

            if (newIndex != currentSpriteIndex) {
                currentSpriteIndex = newIndex;
            }
        }
        string randomName = family.name + " " + currentSpriteIndex;

        foreach (Sprite spr in sprites) {
            if (spr.name == randomName) return spr;
        }

        return null;
    }

    private void Awake() {
        Singletron = this;

        entityLayer = LayerMask.GetMask("Entity");

        sprites = Resources.LoadAll<Sprite>("Ground Sprites");

        Regex rx = new Regex(@" \d$");

        foreach (Sprite sprite in sprites) {
            if (sprite.name.StartsWith("Filled Tile")) continue;

            string familyName = rx.Replace(sprite.name, "");

            //in order to get name of tile, take name of sprite and remove the last number in it (use regex? idk)
            TileFamily family = GetTileFamily(familyName);
            if (family == null) {
                family = new TileFamily() { name = familyName };
                tileFamilies.Add(family);
            }

            family.count++;
        }
    }

    private void Update() {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            Vector2 mousePos = MainCameraController.Singletron.selfCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 screenPos;

            //cancel editing level when pressing escape
            if ((prefab != null || eraser) && Input.GetKeyDown(KeyCode.Escape) && LastPressedEscape.LastPressedEscapeCooldownOver(0.1f)) {
                if (prefab != null && movingEntityData != null) {
                    prefab.transform.position = movingEntityData.GetPosition();
                }
                SetPrefab(null, null);
                SetEraser(false);
                LastPressedEscape.SetPressedEscape();
            }

            //eraser code
            if (Input.GetMouseButton(0) && eraser) {
                RaycastHit2D rayHit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (rayHit.collider != null) {
                    LevelData data = LevelEditorManager.GetLevelData();
                    LevelObject @object = data.GetObjectAtPosition(rayHit.transform.position);

                    if (Input.GetMouseButtonDown(0) && @object is LevelEntity && (((LevelEntity)@object).resourceName == "Ship Pads/Launch Pad" || ((LevelEntity)@object).resourceName == "Ship Pads/Land Pad"))
                    {
                        LevelEditorTutorialManager.TriedToDeletePads();
                    }

                    if (@object != null && (@object.canAdvancedModify)) {
                        LevelEditorLinesController.DestroyAllLinesWithTarget(rayHit.transform);
                        LevelEditorLinesController.DestroyAllLinesWithSource(rayHit.transform);

                        data.levelData.Remove(@object);
                        Destroy(rayHit.transform.gameObject);

                        RefreshTilesLayout();
                    }
                }
            } else {
                //if we have neither a prefab or a sprite selected at the moment and we pressed the left mouse button
                if (prefab == null && Input.GetMouseButtonDown(0) && Time.time > lastMouseClick + 0.2f) {
                    if (Input.GetKey(KeyCode.LeftAlt)) { //if we are holding down the left alt key while left clicking
                        RaycastHit2D rayHit = Physics2D.Raycast(mousePos, Vector2.zero, 10000, entityLayer); //do a raycast

                        if (rayHit.collider != null && rayHit.transform.gameObject.layer == 11) { //if we hit something and it was an entity
                            LevelData data = LevelEditorManager.GetLevelData(); //get the level data
                            LevelObject objectHit = data.GetObjectAtPosition(rayHit.transform.position, true);
                            if (objectHit is LevelEntity)
                            {
                                LevelEntity entity = (LevelEntity)data.GetObjectAtPosition(rayHit.transform.position, true); //find the object we hit

                                if (firstEntityLogicLinking == null)
                                {
                                    if (entity != null && entity.isLogicEntity)
                                    { //safety check to make sure entity is not null
                                        if (entity.isLogicActivator)
                                        { //if this is a logic activator
                                          //create line
                                            LevelEditorLinesController.CreateOrEditLine(rayHit.transform, transform);
                                            firstEntityLogicLinking = entity;
                                            placeTiles = false;
                                        }
                                        else
                                        {
                                            //remove all lines connecting into this
                                        }
                                    }
                                }
                                else
                                {
                                    if (entity != null && entity.isLogicEntity)
                                    { //safety check to make sure entity is not null
                                      //change visual line to lock between new end entity
                                        LevelEditorLinesController.UpdateTarget(transform, rayHit.transform);
                                        firstEntityLogicLinking.logicTarget = entity;
                                        firstEntityLogicLinking = null;
                                    }
                                }
                            }
                        }
                    } else { //if not holding down left alt key
                        RaycastHit2D rayHit = Physics2D.Raycast(mousePos, Vector2.zero, 10000, entityLayer); //do a raycast

                        if (rayHit.collider != null) { //if we hit something
                            LevelData data = LevelEditorManager.GetLevelData(); //get the level data
                            LevelObject @object = data.GetObjectAtPosition(rayHit.transform.position); //find the object we hit

                            if (rayHit.transform.gameObject.layer == 11 && @object != null && @object is LevelEntity) { //if we hit an entity
                                prefab = rayHit.transform.gameObject;
                                movingEntityData = (LevelEntity)@object;
                                lastMouseClick = Time.time;
                            }
                        }
                    }
                }

                //if we pressed the left alt key
                if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                    if (prefab != null && movingEntityData != null) {
                        prefab.transform.position = movingEntityData.GetPosition();
                    }

                    firstEntityLogicLinking = null;
                    SetPrefab(null, null);
                    CursorController.AddUser("EditorLinkLogicEntities", CursorUser.Type.EditorLinkLogicEntities);
                    LevelEditorLinesController.DestroyAllLinesWithTarget(transform);
                }

                //when we let go of the left alt key
                if (Input.GetKeyUp(KeyCode.LeftAlt)) {
                    firstEntityLogicLinking = null;
                    CursorController.RemoveUser("EditorLinkLogicEntities");
                    LevelEditorLinesController.DestroyAllLinesWithTarget(transform);
                }

                //if we have a prefab currently selected
                if (prefab != null && movingEntityData != null) {
                    if (movingEntityData.lockedToGrid) {
                        screenPos = new Vector2(Mathf.Round(transform.parent.InverseTransformPoint(mousePos).x + 0.5f) - 0.5f, Mathf.Round(transform.parent.InverseTransformPoint(mousePos).y + 0.5f) - 0.5f);
                    } else {
                        screenPos = new Vector2(transform.parent.InverseTransformPoint(mousePos).x, transform.parent.InverseTransformPoint(mousePos).y);
                    }

                    if (movingEntityData.canAdvancedModify) {
                        //typical rotation and mirroring things
                        if (movingEntityData.lockedRotation) {
                            if (Input.GetKeyDown(KeyCode.Q)) SetRotation(rotation + 90);
                            if (Input.GetKeyDown(KeyCode.E)) SetRotation(rotation - 90);
                        } else {
                            if (Input.GetKey(KeyCode.Q)) SetRotation(rotation + 5);
                            if (Input.GetKey(KeyCode.E)) SetRotation(rotation - 5);
                        }

                        if (Input.GetKeyDown(KeyCode.A)) ToggleEraser();
                    }

                    //set the moving prefab to cursor location
                    prefab.transform.localPosition = screenPos;

                    //if we press the mouse left click we place down the prefab, store it's location information, and remove it from our variables
                    if (Input.GetMouseButtonDown(0) && Time.time > lastMouseClick + 0.2f) {
                        if (movingEntityData != null) { //if we somehow lost the entity data or something
                            movingEntityData.x = screenPos.x;
                            movingEntityData.y = screenPos.y;
                            movingEntityData.scaleX = prefab.transform.localScale.x;
                            movingEntityData.scaleY = prefab.transform.localScale.y;
                            movingEntityData.rotation = prefab.transform.rotation.eulerAngles.z;

                            prefab.transform.position = movingEntityData.GetPosition();

                            placeTiles = false;
                            movingEntityData = null;
                            prefab = null;
                            lastMouseClick = Time.time;
                        }
                    }
                } else {
                    screenPos = new Vector2(Mathf.Round(transform.parent.InverseTransformPoint(mousePos).x + 0.5f) - 0.5f, Mathf.Round(transform.parent.InverseTransformPoint(mousePos).y + 0.5f) - 0.5f);

                    if (Input.GetKeyDown(KeyCode.Q)) {
                        OffsetRotateTileUnder(90);
                    }
                    if (Input.GetKeyDown(KeyCode.E)) {
                        OffsetRotateTileUnder(-90);
                    }
                    if (Input.GetKeyDown(KeyCode.A)) ToggleEraser();

                    //here we actually place the new tile
                    if (Input.GetMouseButton(0) && placeTiles) {
                        //here we check if we are placing upon a tile, if yes, delete it
                        bool tileRemoved = true;

                        LevelData data = LevelEditorManager.GetLevelData();
                        LevelObject objectAtPos = data.GetObjectAtPosition(screenPos);
                        if (objectAtPos != null) { //if we are using grid and there is already a tile where we are trying to place a tile, replace it
                            if (Input.GetMouseButtonDown(0)) {
                                foreach (GameObject @object in GameObject.FindGameObjectsWithTag("Level")) {
                                    if (Vector2.Distance(@object.transform.position, screenPos) < 0.1) Destroy(@object);
                                }
                                data.levelData.Remove(objectAtPos);
                                tileRemoved = true;
                            } else {
                                tileRemoved = false;
                            }
                        }

                        if (tileRemoved) {
                            //add the tile to the level data
                            LevelTile newTile = new LevelTile(screenPos.x, screenPos.y, 1, 1, rotation, "Filled Tile");
                            data.levelData.Add(newTile);
                            data.InvalidateBounds();

                            newTile.rotation = 0;

                            //spawn the new sprite in the editor
                            SpriteRenderer newSprite = Instantiate(template, transform.parent);
                            newSprite.sprite = sprites.First(x => x.name == "Filled Tile");
                            newSprite.gameObject.layer = 9;
                            newSprite.gameObject.AddComponent(typeof(BoxCollider2D));
                            newSprite.transform.localPosition = screenPos;
                            newSprite.transform.eulerAngles = transform.eulerAngles;
                            newSprite.transform.localScale = transform.localScale;
                            newSprite.gameObject.tag = "Level";
                            LevelObjectHolder objectHolder = newSprite.gameObject.AddComponent<LevelObjectHolder>();
                            objectHolder.levelTile = newTile;

                            RefreshTilesLayout();
                        }

                        RaycastHit2D rayHit = Physics2D.Raycast(mousePos, Vector2.zero);

                        if (rayHit.collider != null) {
                            LevelObject @object = data.GetObjectAtPosition(rayHit.transform.position);
                            if (@object != null && @object.canAdvancedModify && @object.GetType() == typeof(LevelTile)) {
                                ((LevelTile)@object).isTall = Input.GetKey(KeyCode.LeftShift);

                                RefreshTilesLayout();
                            }
                        }
                    }
                }

                if (Input.GetKey(KeyCode.LeftAlt)) {
                    screenPos = new Vector2(transform.parent.InverseTransformPoint(mousePos).x, transform.parent.InverseTransformPoint(mousePos).y);
                }


                transform.localPosition = screenPos;
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse0) && prefab == null && CursorController.GetUser("EditorLinkLogicEntities") == null) {
            placeTiles = true;
        }
    }

    /// <summary>
    /// applies rotational offset to the tile underneath the cursor
    /// </summary>
    private void OffsetRotateTileUnder(float offset) {
        Vector2 mousePos = MainCameraController.Singletron.selfCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D rayHit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (rayHit.collider != null) {
            LevelData data = LevelEditorManager.GetLevelData();
            LevelObject @object = data.GetObjectAtPosition(rayHit.transform.position);
            if (@object != null && @object.canAdvancedModify && @object.GetType() == typeof(LevelTile)) {
                @object.rotationOffset += offset;

                RefreshTilesLayout();
            }
        }
    }

    /// <summary>
    /// Doesn't include other tiles
    /// </summary>
    /// <param name="point"></param>
    private Vector2 GetNeighborTileInLevel(Vector2 point) {
        LevelData levelData = LevelEditorManager.GetLevelData();

        Vector2 above = point + Vector2.up;
        Vector2 below = point - Vector2.up;
        Vector2 right = point + Vector2.right;
        Vector2 left = point - Vector2.right;

        if (levelData.IsPointInLevel(above, false)) return above;
        if (levelData.IsPointInLevel(below, false)) return below;
        if (levelData.IsPointInLevel(right, false)) return right;
        if (levelData.IsPointInLevel(left, false)) return left;

        return point; //return the point itself if we didn't find anything
    }

    /// <summary>
    /// returns true if the point is directly between two tiles (doesn't include corners and edges...)
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private bool IsPointInbetweenTwoTiles(Vector2 point) {
        LevelData levelData = LevelEditorManager.GetLevelData();
        Vector2[] tiles = levelData.GetTiles().ToArray();

        Vector2 above = point + Vector2.up;
        Vector2 below = point - Vector2.up;
        Vector2 right = point + Vector2.right;
        Vector2 left = point - Vector2.right;

        if (tiles.Contains(left) && tiles.Contains(right)) return true;
        if (tiles.Contains(above) && tiles.Contains(below)) return true;

        return false;
    }

    /*private bool GetFlatsNearCorner(Vector2 point, out LevelTile flat1, out LevelTile flat2) {
        LevelData levelData = LevelEditorManager.GetLevelData();
        Vector2[] tiles = levelData.GetTiles().ToArray();

        Vector2 above = point + Vector2.up;
        Vector2 below = point - Vector2.up;
        Vector2 right = point + Vector2.right;
        Vector2 left = point - Vector2.right;

        List<LevelTile> returnTiles = new List<LevelTile>();

        foreach(LevelTile tile in levelData.GetTilesByFamilyName("Flat Ground", "Tall Flat Ground", "Flat To Tall Flat")) {
            if(returnTiles.Count < 2) {
                if (tile.GetPosition() == above || tile.GetPosition() == below || tile.GetPosition() == right || tile.GetPosition() == left) returnTiles.Add(tile);
            }
        }

        if(returnTiles.Count == 2) {
            flat1 = returnTiles[0];
            flat2 = returnTiles[1];
            return true;
        } else {
            flat1 = null;
            flat2 = null;
            return false;
        }
    }*/

    private Vector2 GetCornerNeighborTile(Vector2 point) {
        LevelData levelData = LevelEditorManager.GetLevelData();
        Vector2[] tiles = levelData.GetTiles().ToArray();

        Vector2 above = point + Vector2.up;
        Vector2 below = point - Vector2.up;
        Vector2 right = point + Vector2.right;
        Vector2 left = point - Vector2.right;

        if (tiles.Contains(above) && tiles.Contains(left)) return left;
        if (tiles.Contains(above) && tiles.Contains(right)) return above;
        if (tiles.Contains(below) && tiles.Contains(left)) return below;
        if (tiles.Contains(below) && tiles.Contains(right)) return right;

        return point;
    }

    private Vector2 GetEdgeNeighborTile(Vector2 point) {
        LevelData levelData = LevelEditorManager.GetLevelData();
        Vector2[] tiles = levelData.GetTiles().ToArray();

        Vector2 above = point + Vector2.up;
        Vector2 below = point - Vector2.up;
        Vector2 right = point + Vector2.right;
        Vector2 left = point - Vector2.right;

        if (tiles.Contains(above) && tiles.Contains(left) && !tiles.Contains(right) && !tiles.Contains(below)) return left;
        if (tiles.Contains(above) && tiles.Contains(right) && !tiles.Contains(left) && !tiles.Contains(below)) return above;
        if (tiles.Contains(below) && tiles.Contains(left) && !tiles.Contains(right) && !tiles.Contains(above)) return below;
        if (tiles.Contains(below) && tiles.Contains(right) && !tiles.Contains(left) && !tiles.Contains(above)) return right;

        return point;
    }

    private bool IsTallTileNearby(Vector2 point, out bool twoNearbyTiles, out LevelTile nearbyTile) {
        LevelData levelData = LevelEditorManager.GetLevelData();
        levelData.SortTilesList();

        Vector2 above = point + Vector2.up;
        Vector2 below = point - Vector2.up;
        Vector2 right = point + Vector2.right;
        Vector2 left = point - Vector2.right;

        List<LevelTile> returnTiles = new List<LevelTile>();

        foreach (LevelTile tile in levelData.levelTiles) {
            if (returnTiles.Count < 2) {
                if (tile.GetPosition() == above || tile.GetPosition() == below || tile.GetPosition() == right || tile.GetPosition() == left) {
                    if (tile.isTall) {
                        returnTiles.Add(tile);
                    }
                }
            }
        }

        if(returnTiles.Count == 0) {
            twoNearbyTiles = false;
            nearbyTile = null;
            return false;
        } else if(returnTiles.Count == 1) {
            twoNearbyTiles = false;
            nearbyTile = returnTiles[0];
            return true;
        } else { //two tiles found
            twoNearbyTiles = true;
            nearbyTile = null;
            return true;
        }
    }

    private void RefreshTilesLayout() {
        LevelData data = LevelEditorManager.Singletron.levelData;

        data.SortTilesList();
        data.SortLines();
        data.GetBounds();

        List<GameObject> tilesWithSprites = new List<GameObject>();

        //first foreach loop/pass
        //this simply creates every flat ground tile based on what neighbor positions around this tile are inside the level
        foreach (GameObject tileGameObject in GameObject.FindGameObjectsWithTag("Level")) {
            Vector2 tilePos = tileGameObject.transform.position;
            SpriteRenderer spriteRenderer = tileGameObject.GetComponent<SpriteRenderer>();
            LevelObjectHolder objectHolder = tileGameObject.GetComponent<LevelObjectHolder>();

            tileGameObject.transform.eulerAngles = Vector3.zero;

            if (IsPointInbetweenTwoTiles(tilePos)) { //if this point is directly inbetween two tiles make tile face towards the level
                Vector2 neighborTileInLevel = GetNeighborTileInLevel(tilePos);
                if (neighborTileInLevel != tilePos) { //if this tile has a neighbor tile in level than create flat ground
                    Vector2 dir = neighborTileInLevel - tilePos;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90 + objectHolder.levelTile.rotationOffset;

                    tileGameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    if (objectHolder.levelTile.isTall) {
                        if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Tall Flat Ground") spriteRenderer.sprite = GetRandomSpriteFromFamily("Tall Flat Ground"); //only update the tile sprite if we are changing it to something else
                        tilesWithSprites.Add(tileGameObject);
                        tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                    } else {
                        bool twoNearbyTiles;
                        LevelTile nearbyTallTile;
                        if (IsTallTileNearby(tilePos, out twoNearbyTiles, out nearbyTallTile)) {
                            if (twoNearbyTiles) {
                                if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Tall Flat Ground") spriteRenderer.sprite = GetRandomSpriteFromFamily("Tall Flat Ground");
                                tilesWithSprites.Add(tileGameObject);
                                tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                            } else {
                                if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Flat To Tall Flat") spriteRenderer.sprite = GetRandomSpriteFromFamily("Flat To Tall Flat");
                                tilesWithSprites.Add(tileGameObject);

                                Vector2 inversedPoint = tileGameObject.transform.InverseTransformPoint(nearbyTallTile.GetPosition());
                                inversedPoint = new Vector2(inversedPoint.x * tileGameObject.transform.localScale.x, inversedPoint.y);
                                if (Mathf.Approximately(Mathf.Round(inversedPoint.x), -1)) {
                                    tileGameObject.transform.localScale = new Vector2(-1, tileGameObject.transform.localScale.y);
                                } else {
                                    tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                                }
                            }
                        } else {
                            if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Flat Ground") spriteRenderer.sprite = GetRandomSpriteFromFamily("Flat Ground");
                            tilesWithSprites.Add(tileGameObject);
                            tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                        }
                    }

                    objectHolder.levelTile.rotation = angle; //store angle
                    objectHolder.levelTile.scaleX = tileGameObject.transform.localScale.x;
                    objectHolder.levelTile.scaleY = tileGameObject.transform.localScale.y;
                    objectHolder.levelTile.spriteName = spriteRenderer.sprite.name;
                }
            }
        }

        //second foreach loop/pass
        //this creates every corner/edge
        foreach (GameObject tileGameObject in GameObject.FindGameObjectsWithTag("Level")) {
            Vector2 tilePos = tileGameObject.transform.position;
            SpriteRenderer spriteRenderer = tileGameObject.GetComponent<SpriteRenderer>();
            LevelObjectHolder objectHolder = tileGameObject.GetComponent<LevelObjectHolder>();

            Vector2 neighborTileInLevel = GetNeighborTileInLevel(tilePos);

            if (neighborTileInLevel == tilePos) {
                Vector2 neighborCornerTile = GetCornerNeighborTile(tilePos);

                if (neighborCornerTile != tilePos) { //if we found a corner tile neighbor
                    Vector2 dir = neighborCornerTile - tilePos; //angle to nearest tile
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90 + objectHolder.levelTile.rotationOffset; //adjust the angle a bit...

                    tileGameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    if (objectHolder.levelTile.isTall) {
                        if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Tall-Tall Corner") spriteRenderer.sprite = GetRandomSpriteFromFamily("Tall-Tall Corner");
                        tilesWithSprites.Add(tileGameObject);
                        tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                    } else {
                        bool twoNearbyTiles;
                        LevelTile nearbyTallTile;
                        if (IsTallTileNearby(tilePos, out twoNearbyTiles, out nearbyTallTile)) {
                            if (twoNearbyTiles) {
                                if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Tall-Tall Corner") spriteRenderer.sprite = GetRandomSpriteFromFamily("Tall-Tall Corner");
                                tilesWithSprites.Add(tileGameObject);
                                tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                            } else {
                                if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Tall Corner") spriteRenderer.sprite = GetRandomSpriteFromFamily("Tall Corner");
                                tilesWithSprites.Add(tileGameObject);

                                Vector2 inversedPoint = tileGameObject.transform.InverseTransformPoint(nearbyTallTile.GetPosition());
                                if (Mathf.Approximately(Mathf.Round(inversedPoint.y), 1)) {
                                    tileGameObject.transform.localScale = new Vector2(-1, tileGameObject.transform.localScale.y);
                                    angle -= 90;
                                } else {
                                    tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                                }
                            }
                        } else {
                            if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Corner") spriteRenderer.sprite = GetRandomSpriteFromFamily("Corner");
                            tilesWithSprites.Add(tileGameObject);
                            tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                        }
                    }
;
                    tileGameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    objectHolder.levelTile.rotation = angle; //store angle
                    objectHolder.levelTile.scaleX = tileGameObject.transform.localScale.x;
                    objectHolder.levelTile.scaleY = tileGameObject.transform.localScale.y;
                    objectHolder.levelTile.spriteName = spriteRenderer.sprite.name;
                }
            } else {
                Vector2 neighborEdgeTile = GetEdgeNeighborTile(tilePos);

                if (neighborEdgeTile != tilePos) { //if we found a corner tile neighbor
                    Vector2 dir = neighborEdgeTile - tilePos;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90 + objectHolder.levelTile.rotationOffset;

                    tileGameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    if (objectHolder.levelTile.isTall) {
                        if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Tall-Tall Edge") spriteRenderer.sprite = GetRandomSpriteFromFamily("Tall-Tall Edge");
                        tilesWithSprites.Add(tileGameObject);
                        tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                    } else {
                        bool twoNearbyTiles;
                        LevelTile nearbyTallTile;
                        if (IsTallTileNearby(tilePos, out twoNearbyTiles, out nearbyTallTile)) {
                            if (twoNearbyTiles) {
                                if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Tall-Tall Edge") spriteRenderer.sprite = GetRandomSpriteFromFamily("Tall-Tall Edge");
                                tilesWithSprites.Add(tileGameObject);
                                tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                            } else {
                                if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Tall Edge") spriteRenderer.sprite = GetRandomSpriteFromFamily("Tall Edge");
                                tilesWithSprites.Add(tileGameObject);

                                Vector2 inversedPoint = tileGameObject.transform.InverseTransformPoint(nearbyTallTile.GetPosition());
                                if (Mathf.Approximately(Mathf.Round(inversedPoint.y), -1)) {
                                    tileGameObject.transform.localScale = new Vector2(-1, tileGameObject.transform.localScale.y);
                                    angle -= 90;
                                } else {
                                    tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                                }
                            }
                        } else {
                            if (TileFamily.SpriteNameToFamilyName(spriteRenderer.sprite.name) != "Edge") spriteRenderer.sprite = GetRandomSpriteFromFamily("Edge");
                            tilesWithSprites.Add(tileGameObject);
                            tileGameObject.transform.localScale = new Vector2(1, tileGameObject.transform.localScale.y);
                        }
                    }

                    tileGameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    objectHolder.levelTile.rotation = angle; //store angle
                    objectHolder.levelTile.scaleX = tileGameObject.transform.localScale.x;
                    objectHolder.levelTile.scaleY = tileGameObject.transform.localScale.y;
                    objectHolder.levelTile.spriteName = spriteRenderer.sprite.name;
                }
            }
        }

        foreach (GameObject tileGameObject in GameObject.FindGameObjectsWithTag("Level")) {
            Vector2 tilePos = tileGameObject.transform.position;
            SpriteRenderer spriteRenderer = tileGameObject.GetComponent<SpriteRenderer>();
            LevelObjectHolder objectHolder = tileGameObject.GetComponent<LevelObjectHolder>();

            if (!tilesWithSprites.Contains(tileGameObject)) {
                spriteRenderer.sprite = sprites.First(x => x.name == "Filled Tile");
                objectHolder.levelTile.spriteName = spriteRenderer.sprite.name;
            }
        }
    }

    public static void SetPrefab(LevelEntity entityData, Sprite sprite) {
        SetRotation(0);

        if (entityData != null) {
            SpriteRenderer newPrefab = Instantiate(Singletron.template, Singletron.transform.parent);
            newPrefab.sprite = sprite;
            newPrefab.color = Color.white;
            newPrefab.gameObject.SetActive(true);
            newPrefab.gameObject.AddComponent(typeof(BoxCollider2D));
            newPrefab.gameObject.layer = 11; //layer 11 is entity
            newPrefab.gameObject.tag = "Entity";
            Singletron.prefab = newPrefab.gameObject;
            Singletron.movingEntityData = entityData;

            LevelData data = LevelEditorManager.GetLevelData();
            data.levelData.Add(entityData);
            entityData.ActivatedEditor(newPrefab.gameObject);

            LevelEditorLinesController.DestroyAllLinesWithTarget(Singletron.transform);
            Singletron.firstEntityLogicLinking = null;

            Singletron.placeTiles = false;

            Singletron.SetEraser(false);
        } else {
            //if(Singletron.prefab != null) Destroy(Singletron.prefab);
            Singletron.prefab = null;
        }
    }

    public static void SetRotation(float rotation) {
        Singletron.rotation = rotation;
        Vector3 euler = Singletron.transform.eulerAngles;
        euler.z = rotation;
        Singletron.transform.eulerAngles = euler;

        if (Singletron.prefab != null) {
            Singletron.prefab.transform.eulerAngles = euler;
        }
    }

    public void SetEraser(bool active) {
        eraser = active;

        if (eraser) {
            CursorController.AddUser("editorEraser", CursorUser.Type.EditorEraser);
        } else {
            CursorController.RemoveUser("editorEraser");
        }
    }

    public void ToggleEraser() {
        SetEraser(!eraser);
    }
}
