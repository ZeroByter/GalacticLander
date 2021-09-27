using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using UnityEngine.SceneManagement;
using System.Text;

public class GameEventMenuController : MonoBehaviour {
    [Header("Main form stuff")]
    public Image backgroundImage;
    public Image headerImage;
    public TMP_Text descriptionText;
    public TMP_Text progressionText;
    [Header("The continue button")]
    public Button beginButton;
    [Header("Buttons Text")]
    public TMP_Text continueText;

    private GameEvent currentEvent;

    private void OnEnable() {
        if (SteamManager.Initialized) {
            SteamCallbacks.LobbyChatUpdate_t.RegisterCallback(LobbyStateChanged);
        }
    }

    private void OnDestroy() {
        SteamCallbacks.LobbyChatUpdate_t.UnregisterCallback(LobbyStateChanged);
    }

    private void UpdateCoopProgressionText() {
        if (!NetworkingManager.CurrentLobbyValid) return;

        if (currentEvent == null) return;

        //we update the progress text
        ulong otherPlayer = LobbyMenuController.GetOtherPlayer();
        if (otherPlayer != 0) {
            string otherPlayerName = SteamFriends.GetFriendPersonaName((CSteamID)otherPlayer);
            int otherPlayerProgress = 0;
            int.TryParse(SteamMatchmaking.GetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, (CSteamID)otherPlayer, "eventProgress"), out otherPlayerProgress);

            int nextPlayLevel = Mathf.Min(otherPlayerProgress, GameEvents.CurrentEventCoopProgression);

            beginButton.interactable = nextPlayLevel < currentEvent.numberOfLevels;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("Your galactic mission progression: {0} / {1} ({2}%)", GameEvents.CurrentEventCoopProgression, currentEvent.numberOfLevels, Mathf.RoundToInt((float)GameEvents.CurrentEventCoopProgression / (float)currentEvent.numberOfLevels * 100f)));
            stringBuilder.AppendLine(string.Format("{0}'s galactic mission progress: {1} / {2} ({3}%)", otherPlayerName, otherPlayerProgress, currentEvent.numberOfLevels, Mathf.RoundToInt((float)otherPlayerProgress / (float)currentEvent.numberOfLevels * 100f)));
            if (GameEvents.CurrentEventCoopProgression == 0 || otherPlayerProgress == 0) {
                stringBuilder.AppendLine(string.Format("Will begin on level {0}", nextPlayLevel));
            } else {
                if(nextPlayLevel >= currentEvent.numberOfLevels) {
                    stringBuilder.AppendLine("Congratulations! You have completed this Galactic Mission!");
                } else {
                    stringBuilder.AppendLine(string.Format("Will continue on level {0}", nextPlayLevel));
                }
            }
            progressionText.text = stringBuilder.ToString();
        } else {
            beginButton.interactable = GameEvents.CurrentEventCoopProgression < currentEvent.numberOfLevels;

            progressionText.text = string.Format("Your galactic mission progression: {0} / {1} ({2}%)", GameEvents.CurrentEventCoopProgression, currentEvent.numberOfLevels, Mathf.RoundToInt((float)GameEvents.CurrentEventCoopProgression / (float)currentEvent.numberOfLevels * 100f));
        }
    }

    private void LobbyStateChanged(LobbyChatUpdate_t callback) {
        if (callback.m_ulSteamIDLobby != NetworkingManager.CurrentLobby) return;

        UpdateCoopProgressionText();
    }

    public void OpenMenu() {
        currentEvent = GameEvents.GetCurrentEvent();

        if(currentEvent != null) {
            if (NetworkingManager.CurrentLobbyValid) {
                UpdateCoopProgressionText();
            } else {
                beginButton.interactable = GameEvents.CurrentEventProgression < currentEvent.numberOfLevels;
            }

            backgroundImage.color = Color.Lerp(GameEvents.GetColorFromHex("#393939"), currentEvent.themeColor, 0.25f);

            headerImage.sprite = currentEvent.GetBannerResource();
            descriptionText.text = currentEvent.description;

            if (NetworkingManager.CurrentLobbyValid) { //if we are in a lobby
                bool haveWeVoted = SteamMatchmaking.GetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, SteamUser.GetSteamID(), "levelvoted") == "event";
                if (haveWeVoted) {
                    continueText.text = "Unvote";
                } else {
                    continueText.text = "Vote";
                }
            } else { //if in singleplayer/not in a lobby
                progressionText.text = string.Format("Your pack level progression: {0} / {1} ({2}%)", GameEvents.CurrentEventProgression, currentEvent.numberOfLevels, Mathf.RoundToInt((float)GameEvents.CurrentEventProgression / (float)currentEvent.numberOfLevels * 100f));

                if (GameEvents.CurrentEventProgression == 0) {
                    continueText.text = "Begin";
                } else {
                    if(GameEvents.CurrentEventProgression < currentEvent.numberOfLevels) {
                        continueText.text = "Continue";
                    } else {
                        continueText.text = "Finished!";
                    }
                }
            }
        }
    }

    public void CloseMenu() {
        if (NetworkingManager.CurrentLobbyValid) {
            CanvasTransitionsManager.Singletron.OpenMenu(5);
        } else {
            CanvasTransitionsManager.Singletron.OpenMenu(0);
        }
    }

    //when the user presses the begin/continue button
    public void BeginEvent() {
        if (NetworkingManager.CurrentLobbyValid) { //if we are in a lobby, make player vote for this event
            CSteamID lobby = (CSteamID)NetworkingManager.CurrentLobby;
            bool haveWeVoted = SteamMatchmaking.GetLobbyMemberData(lobby, SteamUser.GetSteamID(), "levelvoted") == "event";
            if (haveWeVoted) {
                SteamMatchmaking.SetLobbyMemberData(lobby, "levelvoted", "");
                continueText.text = "Vote";
            } else {
                SteamMatchmaking.SetLobbyMemberData(lobby, "levelvoted", "event");
                continueText.text = "Unvote";
            }
            SteamMatchmaking.SetLobbyMemberData(lobby, "levelvotedworkshopname", "");
        } else { //if not in lobby, simply start the next level for the event
            int playLevel = GameEvents.CurrentEventProgression;

            playLevel = Mathf.Min(currentEvent.numberOfLevels, playLevel);

            LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.Game, string.Format("{0}/{0}_sp{1}", currentEvent.name, playLevel));
            SceneManager.LoadScene("Game Level");
        }
    }
}
