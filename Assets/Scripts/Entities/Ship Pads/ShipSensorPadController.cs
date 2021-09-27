using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShipSensorPadController : MonoBehaviour {
    public LightBulbController lightsController;
    public TMP_Text text;

    private bool lastIsCaptured = false;
    private bool isCaptured = false;
    private bool isCapturing;
    private float lastStartedCapturing;
    private float maxCapture = 5;

    private AudioSource selfAudio;
    private LevelObjectHolder objectHolder;

    private void Awake() {
        selfAudio = GetComponent<AudioSource>();
    }

    private void Start() {
        objectHolder = GetComponent<LevelObjectHolder>();
        text.text = "SENSOR #" + objectHolder.levelEntity.logicNumber.ToString();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            isCapturing = collision.transform.position.y > transform.position.y;
            lastStartedCapturing = Time.time;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            isCapturing = false;
        }
    }

    private void Update() {
        if (isCapturing) {
            float capturingProgress = Time.time - lastStartedCapturing;
            lightsController.progress = capturingProgress / maxCapture;

            if (lightsController.progress < 0.95f) {
                if (!selfAudio.isPlaying) selfAudio.Play();
                selfAudio.volume = 1;
                selfAudio.pitch = Mathf.Lerp(0, 2, capturingProgress / maxCapture);
            } else {
                selfAudio.volume = Mathf.Lerp(selfAudio.volume, 0, 0.04f);

                if(selfAudio.volume < 0.03f) {
                    selfAudio.Stop();
                }
            }

            if (capturingProgress >= maxCapture && objectHolder != null) {
                objectHolder.levelEntity.ActivateLogic();
                isCaptured = true;
            }
        } else if (isCaptured) {
            lightsController.progress = 1;
        } else {
            lightsController.progress -= Time.deltaTime / maxCapture;

            selfAudio.pitch = lightsController.progress;

            if (lightsController.progress <= 0) {
                lightsController.progress = 0;
                if (objectHolder != null) objectHolder.levelEntity.DeactivateLogic();
                selfAudio.Stop();
            }
        }

        if(lastIsCaptured != isCaptured) {
            if (isCaptured) {
                selfAudio.Play();
            }
        }
        lastIsCaptured = isCaptured;
    }
}
