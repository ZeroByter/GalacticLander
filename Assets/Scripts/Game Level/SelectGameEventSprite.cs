using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SelectGameEventSprite : MonoBehaviour {
    public string spritePath;

    private SpriteRenderer selfRenderer;

    private void Awake() {
        selfRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        if (selfRenderer == null || selfRenderer.sprite == null) return;

        if(LevelLoader.Singletron != null) {
            if (LevelLoader.IsPlayingEventLevel()) {
                GameEvent currentEvent = GameEvents.GetCurrentEvent();

                Sprite eventSprite = Resources.Load<Sprite>(spritePath + "_" + currentEvent.name);
                if(eventSprite != null) {
                    selfRenderer.sprite = eventSprite;
                }
            }
        }

        Destroy(this);
    }
}
