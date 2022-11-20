using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorControlsManager : MonoBehaviour
{
    private static LevelEditorControlsManager Singleton;

    public static void UpdateUI()
    {
        if (Singleton == null) return;

        Singleton._UpdateUI();
    }

    [SerializeField]
    private GameObject brushSelectionIndicator;
    [SerializeField]
    private Slider brushHardnessSlider;
    [SerializeField]
    private Slider brushSizeSlider;
    [SerializeField]
    private Button selectBrushButton;

    [Space]
    [SerializeField]
    private GameObject eraserSelectionIndicator;
    [SerializeField]
    private Button selectEraserButton;

    private void Awake()
    {
        Singleton = this;

        selectBrushButton.onClick.AddListener(HandleSelectBrushClick);
        brushHardnessSlider.onValueChanged.AddListener(HandleBrushHardnessChange);
        brushSizeSlider.onValueChanged.AddListener(HandleBrushSizeChange);

        selectEraserButton.onClick.AddListener(HandleSelectEraserClick);
    }

    private void Start()
    {
        UpdateUI();

        LevelEditorCursor.SetBrushSize(brushSizeSlider.value);
        LevelEditorCursor.SetBrushHardness(brushHardnessSlider.value, brushHardnessSlider.minValue, brushHardnessSlider.maxValue);
    }

    private void OnDestroy()
    {
        selectBrushButton.onClick.RemoveListener(HandleSelectBrushClick);
        brushHardnessSlider.onValueChanged.RemoveListener(HandleBrushHardnessChange);
        brushSizeSlider.onValueChanged.RemoveListener(HandleBrushSizeChange);

        selectEraserButton.onClick.RemoveListener(HandleSelectEraserClick);
    }

    private void HandleSelectBrushClick()
    {
        LevelEditorCursor.SetEraserSelected(false);
    }

    private void HandleBrushHardnessChange(float newValue)
    {
        LevelEditorCursor.SetBrushHardness(newValue, brushHardnessSlider.minValue, brushHardnessSlider.maxValue);
    }

    private void HandleBrushSizeChange(float newSize)
    {
        LevelEditorCursor.SetBrushSize(newSize);
    }

    private void HandleSelectEraserClick()
    {
        LevelEditorCursor.SetEraserSelected(true);
    }

    private void _UpdateUI()
    {
        var eraserSelected = LevelEditorCursor.IsEraserSelected();

        eraserSelectionIndicator.SetActive(eraserSelected);
        brushSelectionIndicator.SetActive(!eraserSelected);
    }
}
