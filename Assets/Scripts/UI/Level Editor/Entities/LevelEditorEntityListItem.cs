using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LevelEditorEntityListItem : MonoBehaviour, IPointerDownHandler
{
    public Image image;
    public TMP_Text itemName;

    private LevelEntity entityData;

    public void Setup(LevelEntity entityData, Sprite sprite, string displayName)
    {
        this.entityData = entityData;
        image.sprite = sprite;
        image.preserveAspect = true;
        itemName.text = displayName;

        gameObject.SetActive(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        SpawnSelectedEntity();
    }

    public void Selected()
    {
        if (LevelEditorCursor.IsCurrentlyMovingObject())
        {
            return;
        }

        SpawnSelectedEntity();
    }

    private void SpawnSelectedEntity()
    {
        LevelEditorCursor.SetPrefab((LevelEntity)entityData.GetDeepCopy(), image.sprite);
    }
}
