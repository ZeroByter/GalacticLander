using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomAudio : MonoBehaviour {
    public AudioSource selfSource;

    public bool destroyWhenDone;

    [Header("The clips")]
    public AudioClip[] clips;

    private void Awake() {
        if (selfSource == null) selfSource = GetComponent<AudioSource>();

        selfSource.clip = clips[Random.Range(0, clips.Length)];
        selfSource.Play();
    }

    private void Update()
    {
        if (destroyWhenDone && !selfSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
