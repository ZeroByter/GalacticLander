using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[ExecuteInEditMode]
public class LerpCanvasGroup : MonoBehaviour {
    [Range(0,1)]
    public float target;
    public float lerp;
    public bool toggleBlockRaycasts = false;
    public float setToZeroThreshold = 0;

    public float currentAlpha {
        get {
            return cGroup.alpha;
        }
    }

    private CanvasGroup cGroup;

    private void Awake() {
        cGroup = GetComponent<CanvasGroup>();
    }

    private void Update() {
        cGroup.alpha = Mathf.Lerp(cGroup.alpha, target, lerp);
        if (toggleBlockRaycasts) {
            cGroup.blocksRaycasts = cGroup.alpha > 0.05f;
        }

        if (cGroup.alpha < setToZeroThreshold) cGroup.alpha = 0;
    }

    public void ForceAlpha(float alpha) {
        if (cGroup == null) return;

        target = alpha;
        cGroup.alpha = alpha;
    }

    public void SetAlpha(float alpha)
    {
        if (cGroup == null) return;

        target = alpha;
    }
}
