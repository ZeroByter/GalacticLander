using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAnimator : MonoBehaviour {
    public float frameTime;
    public Texture2D[] textures;

    private Renderer selfRenderer;
    private float lastChangedFrame;
    private int currentFrame = 0;

    private void Awake() {
        selfRenderer = GetComponent<Renderer>();
    }

    private void Update() {
        selfRenderer.material.mainTexture = textures[currentFrame];

        if(Time.time > lastChangedFrame + frameTime) {
            lastChangedFrame = Time.time;
            currentFrame++;
            if (currentFrame >= textures.Length) currentFrame = 0;
        }
    }
}
