using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LevelEditorEntityListItem : MonoBehaviour {
    public Image image;
    public TMP_Text itemName;

    private LevelEntity entityData;

    public void Setup(LevelEntity entityData, Sprite sprite, string displayName) {
        this.entityData = entityData;
        image.sprite = sprite;
        image.preserveAspect = true;
        itemName.text = displayName;

        gameObject.SetActive(true);
    }

    public void Selected() {
        LevelEditorCursor.SetPrefab((LevelEntity) entityData.GetDeepCopy(), image.sprite);
    }
}
