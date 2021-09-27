using System;
using UnityEngine;

public class LoadAllGroundSprites : MonoBehaviour {

    private void Awake() {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Ground Sprites");
        
        foreach(Sprite sprite in sprites) {
            GameObject go = new GameObject(sprite.name);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            go.AddComponent<AutoTileColliderMaker>();
        }
    }
}
