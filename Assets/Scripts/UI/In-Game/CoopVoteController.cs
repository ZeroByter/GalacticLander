using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Steamworks;
using TMPro;

public class CoopVoteController : MonoBehaviour {
    private static CoopVoteController Singletron;

    public LerpCanvasGroup lerpGroup;
    public CanvasGroup buttonsGroup;
    public TMP_Text voteText;

    public enum VoteType {
        RestartLevel = 0,
        GotoNextLevel = 1,
        BackToLobby = 2
    }

    private bool voteActive = false;
    private ulong initiatedVote;
    private VoteType currentVote;

    private void Awake() {
        Singletron = this;

        CursorController.RemoveUser("coopVote");

        lerpGroup.ForceAlpha(0);
    }

    private string GetNameOfInitiatedVote() {
        if (initiatedVote == 0) return "";
        return SteamFriends.GetFriendPersonaName((CSteamID)initiatedVote);
    }

    private string VoteTypeToString() {
        switch (currentVote) {
            case VoteType.BackToLobby:
                return string.Format("{0} wants to go back to lobby", GetNameOfInitiatedVote());
            case VoteType.GotoNextLevel:
                return string.Format("{0} wants to go to the next level", GetNameOfInitiatedVote());
            case VoteType.RestartLevel:
                return string.Format("{0} wants to restart the level", GetNameOfInitiatedVote());
            default:
                return "";
        }
    }

    public static void StartVote(VoteType typeOfVote) {
        if(Singletron.voteActive && Singletron.currentVote == typeOfVote && Singletron.initiatedVote != (ulong)SteamUser.GetSteamID()) {
            Singletron.AgreeToVote();
        } else {
            NetworkingManager.SendPacket(new object[] { 8, (int)typeOfVote }, 1);
        }
    }

    public void AgreeToVote() {
        NetworkingManager.SetSceneNotLoaded();
        NetworkingManager.SendPacket(new object[] { 9, true }, 1, EP2PSend.k_EP2PSendReliableWithBuffering);
    }

    public void DisagreeToVote() {
        NetworkingManager.SendPacket(new object[] { 9, false }, 1);
    }

    public static void DisplayNewVote(ulong initiated, VoteType voteType) {
        if (Singletron == null) return;
        
        Singletron.voteActive = true;
        Singletron.lerpGroup.target = 1;

        Singletron.initiatedVote = initiated;
        Singletron.currentVote = voteType;

        Singletron.voteText.text = Singletron.VoteTypeToString();
        print(Singletron.VoteTypeToString());

        if(Singletron.initiatedVote != (ulong)SteamUser.GetSteamID()) {
            CursorController.AddUser("coopVote");
        }

        Singletron.buttonsGroup.interactable = Singletron.initiatedVote != (ulong)SteamUser.GetSteamID();
    }

    public static void VoteResponse(bool agree) {
        if (!Singletron.voteActive) return; //if there is no active vote, do nothing
        if (SceneManager.GetActiveScene().name != "Game Level") return; //if for some reason some how we are not in the game level scene, do nothing

        if (agree) {
            switch (Singletron.currentVote) {
                case VoteType.RestartLevel:
                    NetworkingManager.ResetNetwork();

                    if (LevelLoader.IsPlayingEventLevel() && VictoryMenuController.LevelCompleted) {
                        GameEvents.CurrentEventCoopProgression--;
                    }

                    SceneManager.LoadScene("Game Level");
                    break;
                case VoteType.BackToLobby:
                    BackToLobbyIfPeerDisconnects.SendPeerBackToLobbyPacket();
                    SceneManager.LoadScene("Main Menu");
                    break;
                case VoteType.GotoNextLevel:
                    NetworkingManager.ResetNetwork();

                    if (LevelLoader.IsPlayingEventLevel()) {
                        GameEvent currentEvent = GameEvents.GetCurrentEvent();

                        LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.Game, string.Format("{0}/{0}_mp{1}", currentEvent.name, Mathf.Min(currentEvent.numberOfLevels, GameEvents.CurrentEventCoopProgression)));
                        SceneManager.LoadScene("Game Level");
                    } else {
                        LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.Game, "mp" + (VictoryMenuController.Singletron.currentLevelNumber + 1));
                        SceneManager.LoadScene("Game Level");
                    }
                    break;
            }
        } else {
            Singletron.voteActive = false;
            Singletron.lerpGroup.target = 0;
            CursorController.RemoveUser("coopVote");
        }
    }
}
