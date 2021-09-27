using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionsController : MonoBehaviour {
    private static ExplosionsController Singletron;

    public Transform[] effects;

    private void Awake() {
        Singletron = this;
    }

    public static void CreateExplosion(int explosionId, Vector2 position) {
        if (Singletron == null) return;

        Instantiate(Singletron.effects[explosionId], position, Quaternion.identity);
    }
}
