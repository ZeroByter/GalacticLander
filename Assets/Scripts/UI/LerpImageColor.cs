using UnityEngine;
using UnityEngine.UI;

public class LerpImageColor : MonoBehaviour {
    public Color target = Color.white;
    [Range(0, 1)]
    public float lerp = 0.5f;

    private Image image;

    private void Awake() {
        image = GetComponent<Image>();

        ForceColor();
    }

    private void Update() {
        image.color = Color.Lerp(image.color, target, lerp);
    }

    public void ForceColor()
    {
        image.color = target;
    }
}
