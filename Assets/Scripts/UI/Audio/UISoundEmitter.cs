using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UISoundEmitter : MonoBehaviour {
    private static UISoundEmitter Singletron;

    public AudioClip[] clips;

    private AudioSource selfAudio;

    private void Awake() {
        if(Singletron != null) {
            Destroy(gameObject);
            return;
        }

        Singletron = this;
        DontDestroyOnLoad(gameObject);

        selfAudio = GetComponent<AudioSource>();
    }

    public static void PlaySound(int index) {
        Singletron.selfAudio.Stop();
        Singletron.selfAudio.clip = Singletron.clips[index];
        Singletron.selfAudio.Play();
    }
}
