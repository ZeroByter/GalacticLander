using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelTile : LevelObject {
    public bool isTall;

    public LevelTile(float x, float y, float scaleX, float scaleY, float rotation, string spriteName) {
        id = newId;
        this.x = x;
        this.y = y;
        this.scaleX = scaleX;
        this.scaleY = scaleY;
        this.rotation = rotation;
        this.spriteName = spriteName;

        newId++;
    }
}
