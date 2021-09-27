using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour {
    private static TooltipController Singletron;

    private float lastActivated = -20;

    private RectTransform rect;
    private TMP_Text text;
    private CanvasGroup group;

    private void Awake() {
        if(Singletron != null) {
            Destroy(transform.parent.gameObject);
            return;
        }

        DontDestroyOnLoad(transform.parent.gameObject);
        Singletron = this;

        rect = GetComponent<RectTransform>();
        text = GetComponentInChildren<TMP_Text>();
        group = GetComponent<CanvasGroup>();
    }

    private void Update() {
        group.alpha = (lastActivated + 1.4f) - Time.time;

        Vector2 newPivot = rect.pivot;
        if (rect.position.x < rect.rect.width) {
            newPivot.x = 0;
        } else {
            newPivot.x = 1;
        }
        rect.pivot = newPivot;
    }

    public static void ActivateTooltip(string text) {
        Singletron.rect.position = Input.mousePosition + new Vector3(20, 0);
        Singletron.text.text = text;
        Singletron.lastActivated = Time.time;
    }
}
