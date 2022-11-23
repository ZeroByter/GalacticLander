using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoorController : MonoBehaviour {
    [Header("The top and bottom texts")]
    public TMP_Text topText;
    public TMP_Text bottomText;

    [Header("The two door slides")]
    public Transform topSlide;
    public Transform bottomSlide;

    [Header("Is the door currently open?")]
    public bool isOpen;

    private LevelEntity levelEntity;

    private bool lastOpen;
    private AudioSource doorSound;

    private void Awake() {
        doorSound = GetComponent<AudioSource>();
    }

    private void Start() {
        LevelObjectHolder entityHolder = GetComponent<LevelObjectHolder>();
        if (entityHolder != null && entityHolder.levelEntity != null) {
            levelEntity = entityHolder.levelEntity;

            string logicNumbers = "";

            foreach(LevelEntity source in LevelLoader.Singletron.levelData.GetLogicSources(levelEntity)) {
                logicNumbers += "#" + source.logicNumber + " ";
            }
            logicNumbers.TrimEnd(' ');

            topText.text = logicNumbers;
            bottomText.text = topText.text;
        }
    }

    private void Update() {
        float lerpSpeed = 1.5f * Time.deltaTime;
        Transform topTransform = topSlide.transform;
        Transform bottomTransform = bottomSlide.transform;

        isOpen = LevelLoader.Singletron.levelData.GetPercentOfActivatedSources(levelEntity) == 1f;

        if (isOpen) {
            topTransform.localScale = new Vector3(1, Mathf.Lerp(topTransform.localScale.y, 0, lerpSpeed), 1);

            topTransform.localPosition = new Vector3(0, Mathf.Lerp(topTransform.localPosition.y, 1.5f, lerpSpeed), 0);
            bottomTransform.localPosition = new Vector3(0, Mathf.Lerp(bottomTransform.localPosition.y, -1.5f, lerpSpeed), 0);
        } else {
            topTransform.localScale = new Vector3(1, Mathf.Lerp(topTransform.localScale.y, 1.5f, lerpSpeed), 1);

            topTransform.localPosition = new Vector3(0, Mathf.Lerp(topTransform.localPosition.y, 0.75f, lerpSpeed), 0);
            bottomTransform.localPosition = new Vector3(0, Mathf.Lerp(bottomTransform.localPosition.y, -0.75f, lerpSpeed), 0);
        }
        bottomTransform.localScale = topTransform.localScale;

        if(lastOpen != isOpen) {
            if (isOpen) {
                doorSound.timeSamples = 0;
                doorSound.pitch = 1;
                doorSound.Play();
            } else {
                doorSound.timeSamples = doorSound.clip.samples - 1;
                doorSound.pitch = -1; //reverse the sound
                doorSound.Play();
            }
        }
        lastOpen = isOpen;
    }
}
