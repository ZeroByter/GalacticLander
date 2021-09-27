using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnfinishedGameText : MonoBehaviour {
    private static bool Instantiated = false;

    private void Awake() {
        if (Instantiated) {
            Destroy(gameObject);
            return;
        }

        Instantiated = true;
        DontDestroyOnLoad(gameObject);
    }
}
