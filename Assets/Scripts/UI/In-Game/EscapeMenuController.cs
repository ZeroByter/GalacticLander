using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeMenuController : MonoBehaviour {
    public static EscapeMenuController Singletron;

    [Header("The transition component")]
    public CanvasBlurTransition transition;

    public bool isOpen {
        get {
            return transition.isOpen;
        }
    }

    private void Awake() {
        Singletron = this;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && LastPressedEscape.LastPressedEscapeCooldownOver(0.1f)) {
            transition.ToggleMenu();

            if (isOpen && LevelLoader.PlayTestingLevel) { //if we are playtesting this level
                SceneManager.LoadScene("Level Editor");
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !SourceConsole.UI.ConsoleCanvasController.IsVisible()) {
            LevelLoader.Singletron.RestartLevel();
        }
    }

    public void OpenSettings() {
        SettingsMenuManager.OpenMenu();
    }

    public void MainMenu() {
        if(SteamManager.Initialized && NetworkingManager.CurrentLobbyValid) { //if we are in coop
            CoopVoteController.StartVote(CoopVoteController.VoteType.BackToLobby);
        } else {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
