using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternatingSpriteController : MonoBehaviour {
    private Sprite sprite1;
    private Sprite sprite2;

    private SpriteRenderer spriteRenderer;

    private int currentFrame;
    private float lastSpriteChange;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();

        sprite1 = Resources.Load<Sprite>("Landing Pads/Landing Pad Blue Light");
        sprite2 = Resources.Load<Sprite>("Landing Pads/Landing Pad Red Light");

        spriteRenderer.sprite = sprite2;
    }

    private void Update() {
        if (Time.time > lastSpriteChange + 0.8f) {
            lastSpriteChange = Time.time;

            if (currentFrame == 0) {
                spriteRenderer.sprite = sprite1;
                currentFrame++;
            } else {
                spriteRenderer.sprite = sprite2;
                currentFrame = 0;
            }
        }
    }
}
