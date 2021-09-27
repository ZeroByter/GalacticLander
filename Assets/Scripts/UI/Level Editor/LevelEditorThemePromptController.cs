using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LevelEditorThemePromptController : MonoBehaviour {
    [Header("mist size slider")]
    public Slider mistSizeSlider;

    [Header("color sliders")]
    public Slider redColor;
    public Slider greenColor;
    public Slider blueColor;

    [Header("preivew images")]
    public Image backgroundPreview;
    public Image mistPreview;
    public Image tilesPreview;

    private void Awake() {
        Setup(1.5f, new Color32(45, 45, 45, 255));
    }

    public void UpdatePreview() {
        //changing mist size
        RectTransform rectTransform = mistPreview.rectTransform;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Lerp(85, 194, mistSizeSlider.value / mistSizeSlider.maxValue));

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.sizeDelta.y / 2);

        //changing tiles colors
        Color tilesColor = new Color32((byte)redColor.value, (byte)greenColor.value, (byte)blueColor.value, 255);

        tilesPreview.color = tilesColor;
        backgroundPreview.color = LevelLoader.DarkerTileColor(tilesColor);
    }

    public void UpdateLevelData() {
        LevelData levelData = LevelEditorManager.GetLevelData();
        if(levelData != null) {
            levelData.useCustomTilesColor = true;
            levelData.backgroundMistSize = mistSizeSlider.value;
            levelData.tilesColor = new Color(redColor.value / 255, greenColor.value / 255, blueColor.value / 255);
        }
    }

    public void Setup(float mistSize, Color color) {
        mistSizeSlider.value = mistSize;

        redColor.value = color.r * 255;
        greenColor.value = color.g * 255;
        blueColor.value = color.b * 255;

        UpdatePreview();
    }
}
