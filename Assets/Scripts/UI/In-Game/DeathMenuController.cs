using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Steamworks;
using System.Collections;

public class DeathMenuController : MonoBehaviour {
    public static DeathMenuController Singletron;

    public TMP_Text deathText;
    public CanvasBlurTransition transition;

    [HideInInspector]
    public bool isOpen {
        get {
            return transition.isOpen;
        }
    }

    private void Awake() {
        Singletron = this;
    }

    private void OnDestroy() {
        if (VictoryMenuController.Singletron != null && VictoryMenuController.Singletron.isOpen) return;
        if (LevelLoader.Singletron == null || LevelLoader.Singletron.levelData == null) return;

        if (!LevelLoader.PlayTestingLevel) {
            //setting up basic variables
            string levelName = LevelLoader.GetLevelDirectory();
            LevelData currentLevelData = LevelLoader.Singletron.levelData;

            if (currentLevelData.workshopId != 0) { //if we left a workshop level
                SteamUGC.StopPlaytimeTrackingForAllItems();
            }

            LevelProgress.LevelType levelType = LevelProgress.LevelType.Game;

            if (levelName.EndsWith(".level")) { //file is external/workshop (since workshop files are stored externally
                levelType = LevelProgress.LevelType.External;
            } else { //otherwise file is game-provided
                levelType = LevelProgress.LevelType.Game;
            }

            //here we actually update the progress arccordingly
            LevelProgress newProgress = LevelProgressCounter.GetProgress(levelType, levelName, LevelLoader.GravityEnabled);

            if (newProgress == null) { //if the progress file doesn't exist we create a new one
                newProgress = new LevelProgress();

                newProgress.gravityEnabled = LevelLoader.GravityEnabled;

                if (levelType == LevelProgress.LevelType.Game) {
                    newProgress.SetAssetLevel(levelName);
                }
                if (levelType == LevelProgress.LevelType.External) {
                    if (currentLevelData.workshopId != 0) { //if this level is a workshop level
                        newProgress.SetExternalLevel("workshop:" + currentLevelData.workshopId);
                    } else {
                        newProgress.SetExternalLevel(levelName);
                    }
                }
            }

            if (newProgress.timesCompleted == 0) {
                newProgress.triesBeforeSuccess++;

                LevelProgressCounter.UpdateOrCreateProgress(newProgress);
            }
        }
    }

    public IEnumerator OpenMenuCoroutine()
    {
        yield return new WaitForSeconds(3.5f);
        
        if (LevelLoader.PlayTestingLevel)
        {
            SceneManager.LoadScene("Level Editor");
            yield return null;
        }

        if (!transition.isOpen)
        {
            GhostReplayPlayback.Singleton.PlayCustomReplay(GhostReplayRecorder.Singleton.GetCurrentGhostReplayDeepCopy(), 1, true, true);

            foreach(var crate in CrateController.Crates)
            {
                crate.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        transition.OpenMenu();

        if (NetworkingManager.CurrentLobbyValid)
        { //if we are in a lobby
            deathText.text = "You both died! Try again!";
        }
        else
        {
            deathText.text = "You died! Try again!";
        }
    }

    public static void OpenMenu() {
        if (Singletron == null) return;
        if (Singletron.transition.isOpen) return;

        Singletron.StartCoroutine(Singletron.OpenMenuCoroutine());
    }
}
