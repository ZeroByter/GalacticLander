using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class ChangeCanvasGroupAlphaWithDisabledButton : MonoBehaviour
{
    [Range(0f, 1f)]
    public float disabledAlpha;

    private CanvasGroup canvasGroup;
    private Button button;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        button = GetComponent<Button>();
    }

    private void Update()
    {
        if (button == null) return;

        if (button.interactable)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = disabledAlpha;
        }
    }
}
