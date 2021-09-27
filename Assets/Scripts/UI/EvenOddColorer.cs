using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class EvenOddColorer : MonoBehaviour{
    [Header("Even and odd colors")]
    public Color evenColor = Color.white;
    public Color oddColor = Color.white;

    private Image image;

    private void Awake() {
        image = GetComponent<Image>();
    }

    private void Update() {
        if (transform.GetSiblingIndex() % 2 != 0) {
            image.color = oddColor;
        } else {
            image.color = evenColor;
        }
    }
}
