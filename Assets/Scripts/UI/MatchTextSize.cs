using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class MatchTextSize : MonoBehaviour {
    public TMP_Text parentText;
    public bool alsoMatchText = false;

    private TMP_Text selfText;

    private void Awake() {
        selfText = GetComponent<TMP_Text>();
    }

    private void Update() {
        if (selfText == null || parentText == null) return;

        selfText.fontSize = parentText.fontSize;

        if (alsoMatchText) {
            selfText.text = parentText.text;
        }
    }
}
