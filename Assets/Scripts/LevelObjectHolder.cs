using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LevelObjectHolder : MonoBehaviour {
    public LevelData levelData {
        get {
            if (LevelLoader.Singletron == null) return null;
            if (LevelLoader.Singletron.levelData == null) return null;

            return LevelLoader.Singletron.levelData;
        }
    }
    
    [HideInInspector]
    public LevelObject levelObject;
    [HideInInspector]
    public LevelTile levelTile;
    [HideInInspector]
    public LevelEntity levelEntity;
}
