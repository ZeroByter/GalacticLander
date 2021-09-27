using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public abstract class LevelObject {

    public static int newId;

    public int id;
    public float x;
    public float y;
    public float scaleX = 1;
    public float scaleY = 1;
    public float rotation;
    public string spriteName;

    public bool isEntity = false;

    /// <summary>
    /// This offset is defined by the user creating the level
    /// </summary>
    public float rotationOffset;

    public bool canOverrideDelete = false;
    public bool canAdvancedModify = true;

    /// <summary>
    /// This field is not serialized, and is set manually everytime this levelObject is loaded/spawned in
    /// </summary>
    [NonSerialized]
    public GameObject gameObject;

    public Vector2 GetPosition() {
        return new Vector2(x, y);
    }

    public Vector3 GetQuaternion() {
        return new Vector3(0, 0, rotation);
    }

    public Vector3 GetScale() {
        return new Vector3(scaleX, scaleY, 1);
    }

    public LevelObject GetDeepCopy() {
        using (var ms = new MemoryStream()) {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;

            return (LevelObject)bf.Deserialize(ms);
        }
    }
}
