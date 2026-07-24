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
    private Button selectRemoveBrushButton;
    [SerializeField]
    private GameObject selectRemoveBrushIndicator;
    [SerializeField]
    private Button selectAddBrushButton;
    [SerializeField]
    private GameObject selectAddBrushIndicator;
    [SerializeField]
    private Slider brushHardnessSlider;
    [SerializeField]
    private Slider brushSizeSlider;

    [Space]
    [SerializeField]
    private GameObject eraserSelectionIndicator;
    [SerializeField]
    private Button selectEraserButton;

    [Space]
    [SerializeField]
    private GameObject linkerSelectionIndicator;
    [SerializeField]
    private Button selectLinkerButton;

    private void Awake()
    {
        Singleton = this;

        selectRemoveBrushButton.onClick.AddListener(HandleSelectRemoveBrushClick);
        selectAddBrushButton.onClick.AddListener(HandleSelectAddBrushClick);
        selectLinkerButton.onClick.AddListener(HandleSelectLinkerClick);
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

    private void HandleSelectRemoveBrushClick()
    {
        LevelEditorCursor.SelectRemoveTerrainTool();
    }

    private void HandleSelectAddBrushClick()
    {
        LevelEditorCursor.SelectAddTerrainTool();
    }

    private void HandleSelectLinkerClick()
    {
        LevelEditorCursor.SelectLinkerTool();
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
        var brushSelected = LevelEditorCursor.IsAddTerrainSelected() || LevelEditorCursor.IsRemoveTerrainSelected();

        selectRemoveBrushIndicator.SetActive(LevelEditorCursor.IsRemoveTerrainSelected());
        selectAddBrushIndicator.SetActive(LevelEditorCursor.IsAddTerrainSelected());

        brushSelectionIndicator.SetActive(brushSelected);
        eraserSelectionIndicator.SetActive(LevelEditorCursor.IsEraserSelected());
        linkerSelectionIndicator.SetActive(LevelEditorCursor.IsLinkerSelected());
    }

    private void OnDestroy()
    {
        selectRemoveBrushButton.onClick.RemoveListener(HandleSelectRemoveBrushClick);
        selectAddBrushButton.onClick.RemoveListener(HandleSelectAddBrushClick);
        selectLinkerButton.onClick.RemoveListener(HandleSelectLinkerClick);
        brushHardnessSlider.onValueChanged.RemoveListener(HandleBrushHardnessChange);
        brushSizeSlider.onValueChanged.RemoveListener(HandleBrushSizeChange);

        selectEraserButton.onClick.RemoveListener(HandleSelectEraserClick);
    }
}
