using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicController : MonoBehaviour {
    public static BackgroundMusicController Singletron;

    public AudioClip[] music;

    private int lastSong;
    private AudioSource audioSource;

    private void Awake() {
        if (Singletron != null) {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();

        Singletron = this;
        DontDestroyOnLoad(gameObject);

        PlayRandom();
    }

    void PlayRandom() {
        if (music.Length <= 1) return;

        int randomIndex = 0;

        while (true) {
            randomIndex = Random.Range(0, music.Length);
            if (randomIndex != lastSong) {
                break;
            }
        }

        lastSong = randomIndex;

        AudioClip randomSong = music[randomIndex];
        audioSource.clip = randomSong;
        audioSource.Play();

        StartCoroutine(PlayNextSong());
    }

    IEnumerator PlayNextSong() {
        yield return new WaitUntil(() => !audioSource.isPlaying);

        PlayRandom();
    }
}
