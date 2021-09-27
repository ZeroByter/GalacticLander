using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightBulbController : MonoBehaviour {
    [Range(0,1)]
    public float progress;
    public SpriteRenderer[] lights;
    [Header("Colors")]
    public Color activatedColor = Color.white;
    public Color deactivatedColor = Color.white;

    private void Update() {
        for(int i = 0; i < lights.Length; i++) {
            SpriteRenderer light = lights[i];

            light.color = Color.Lerp(deactivatedColor, activatedColor, progress * lights.Length - i);
        }
    }
}
