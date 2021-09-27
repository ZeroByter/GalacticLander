using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteAfterTimer : MonoBehaviour {
    [Range(0, 10)]
    public float liveTime = 2;

    private float timeCreated;

    private void Awake() {
        timeCreated = Time.time;
    }

    private void Update() {
        if(Time.time > timeCreated + liveTime) {
            Destroy(gameObject);
        }
    }
}
