using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialOffsetter : MonoBehaviour {
    public Vector2 offset;

    private Renderer selfRenderer;

    private void Awake() {
        selfRenderer = GetComponent<Renderer>();
    }

    private void Update() {
        selfRenderer.material.mainTextureOffset += offset;
    }
}
