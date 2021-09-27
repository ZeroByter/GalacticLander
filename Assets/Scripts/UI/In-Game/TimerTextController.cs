using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerTextController : MonoBehaviour {
    public LerpCanvasGroup lerpGroup;
    public TMP_Text currentTimer;
    public TMP_Text bestTime;

    private float stopCurrentTimerTime;

    private bool ShowLevelTimer {
        get {
            return PlayerPrefs.GetInt("showLevelTimer", 1) == 1;
        }
    }
    private bool shouldShowLevelTimer;

    private void Awake() {
        lerpGroup.ForceAlpha(0);
        shouldShowLevelTimer = ShowLevelTimer;
    }

    private void Start() {
        string levelName = LevelLoader.GetLevelDirectory();
        LevelProgress.LevelType levelType = LevelProgress.LevelType.Game;

        if (levelName.EndsWith(".level")) { //file is external/workshop (since workshop files are stored externally
            levelType = LevelProgress.LevelType.External;
        } else { //otherwise file is game-provided
            levelType = LevelProgress.LevelType.Game;
        }

        LevelProgress newProgress = LevelProgressCounter.GetProgress(levelType, levelName, LevelLoader.GravityEnabled);
        if(newProgress != null && newProgress.bestTime != 0) {
            bestTime.text = "Best Time: " + newProgress.bestTime;
        } else {
            bestTime.text = "";
        }
    }

    private float GetTimeSinceFirstMovement() {
        if(stopCurrentTimerTime == 0) {
            if (FirstMovementTimer.GetFirstMovementTime() == 0) return 0;

            return FirstMovementTimer.GetTimeSinceFirstMovement();
        } else {
            return stopCurrentTimerTime - FirstMovementTimer.GetFirstMovementTime();
        }
    }

    private void Update() {
        if(shouldShowLevelTimer && GetTimeSinceFirstMovement() != 0) {
            currentTimer.text = "Timer: " + GetTimeSinceFirstMovement();

            lerpGroup.target = 1;
        } else {
            lerpGroup.target = 0;
        }

        if(stopCurrentTimerTime == 0 && (VictoryMenuController.Singletron.isOpen || DeathMenuController.Singletron.isOpen)) {
            stopCurrentTimerTime = Time.time;
        }
    }
}
