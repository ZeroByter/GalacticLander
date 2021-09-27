using System;
using UnityEngine;

[Serializable]
public class LevelEntity : LevelObject {
    public string resourceName;
    public object[] customProperties;

    /// <summary>
    /// Is this a logic entity (regardless of activator/receiver)
    /// </summary>
    public bool isLogicEntity;
    /// <summary>
    /// Is this a logic sensor/activator
    /// </summary>
    public bool isLogicActivator;
    /// <summary>
    /// If the entity is a logic activator, this determines if it is currently activated. All activators start as false.
    /// </summary>
    public bool isLogicActivated;
    /// <summary>
    /// If logic sensor/activator, then what is this logic activator activating?
    /// </summary>
    public LevelEntity logicTarget;
    /// <summary>
    /// The visual number represented on this logic entity in-game
    /// </summary>
    public int logicNumber;

    public bool lockedToGrid;
    public bool lockedRotation;

    public string[] randomResourceNames;

    public LevelEntity(string resourceName, object[] customProperties) {
        this.isEntity = true;
        this.resourceName = resourceName;
        this.customProperties = customProperties;
        this.randomResourceNames = new string[0];
    }

    public LevelEntity(string resourceName) {
        this.isEntity = true;
        this.resourceName = resourceName;
        this.customProperties = new object[0];
        this.randomResourceNames = new string[0];
    }

    public LevelEntity(params string[] randomNames) {
        this.isEntity = true;
        this.resourceName = "";
        this.customProperties = new object[0];
        this.randomResourceNames = randomNames;
    }

    /// <summary>
    /// Called by the level editor when this entity/prop has been instantiated inside the editor (and not in-game)
    /// </summary>
    /// <param name="editorObject"></param>
    public virtual void ActivatedEditor(GameObject editorObject) {

    }

    public virtual void ActivateLogic() {
        isLogicActivated = true;
    }

    public virtual void DeactivateLogic() {
        isLogicActivated = false;
    }
}
