using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteRedFlasher : MonoBehaviour {
    public float flashSpeed = 0.25f;
    public float flashTime = 1f;

    private SpriteRenderer sprite;
    private float timeCreated;
    private float i;

    private float timeSinceCreation {
        get {
            return Time.time - timeCreated;
        }
    }

    private void Awake() {
        sprite = GetComponent<SpriteRenderer>();
        timeCreated = Time.time;

        if(sprite == null) {
            Destroy(this);
            return;
        }
    }

    private void Update() {
        if(timeSinceCreation > flashTime) {
            Destroy(this); //delete self component
            return;
        }

        i += flashSpeed * Time.deltaTime;

        float c = (Mathf.Sin(i) + 1) / 2;
        sprite.color = new Color(Mathf.Lerp(1, c, c), 1 - c, 1 - c, 1);
    }

    private void OnDisable() {
        if (sprite == null) return;

        sprite.color = Color.white;
    }
}
