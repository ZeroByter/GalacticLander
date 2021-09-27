using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ShipComponentCollisionSound : MonoBehaviour {
    public AudioSource soundSource;

    private AudioClip[] lightCollisions;
    private AudioClip[] heavyCollisions;

    private void Awake() {
        if (soundSource == null) soundSource = GetComponent<AudioSource>();

        lightCollisions = new AudioClip[3];
        for(int i = 0; i < lightCollisions.Length; i++) {
            lightCollisions[i] = Resources.Load<AudioClip>("Ship Light Collision " + i);
        }

        heavyCollisions = new AudioClip[5];
        for (int i = 0; i < heavyCollisions.Length; i++) {
            heavyCollisions[i] = Resources.Load<AudioClip>("Ship Heavy Collision " + i);
        }
    }

    private AudioClip GetRandomLightClip() {
        return lightCollisions[Random.Range(0, lightCollisions.Length - 1)];
    }

    private AudioClip GetRandomHeavyClip() {
        return heavyCollisions[Random.Range(0, heavyCollisions.Length - 1)];
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (soundSource.isPlaying) return;
        if (collision.relativeVelocity.magnitude <= 1f) return;
        if (collision.gameObject.tag == "Player") return;

        if(collision.relativeVelocity.magnitude <= 2) {
            soundSource.clip = GetRandomLightClip();
            soundSource.Play();
        } else {
            soundSource.clip = GetRandomHeavyClip();
            soundSource.Play();
        }
    }
}
