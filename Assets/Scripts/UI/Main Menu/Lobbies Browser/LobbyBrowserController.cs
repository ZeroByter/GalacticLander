using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using TMPro;
using System;

public class LobbyBrowserController : MonoBehaviour {
    [HideInInspector]
    public ulong lobbyId;

    public TMP_Text lobbyName;
    public TMP_Text playerCount;
    //public TMP_Text ping;

    private void OnEnable() {
        LobbyBrowserManager.UpdateLobbiesNames += UpdateLobbyName;
    }

    private void OnDestroy() {
        LobbyBrowserManager.UpdateLobbiesNames -= UpdateLobbyName;
    }

    private void UpdateLobbyName() {
        lobbyName.text = SteamMatchmaking.GetLobbyData((CSteamID)lobbyId, "name");
        playerCount.text = SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyId) + " / " + SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyId);
    }

    public void Setup(ulong lobbyId) {
        this.lobbyId = lobbyId;

        gameObject.SetActive(true);

        lobbyName.text = SteamMatchmaking.GetLobbyData((CSteamID)lobbyId, "name");
        playerCount.text = SteamMatchmaking.GetNumLobbyMembers((CSteamID)lobbyId) + " / " + SteamMatchmaking.GetLobbyMemberLimit((CSteamID)lobbyId);
    }

    private bool joiningLobby;

    public void JoinLobby() {
        if (SteamManager.Initialized && !joiningLobby) {
            joiningLobby = true;

            SteamCallbacks.LobbyEnter_t.RegisterCallResult(OnJoinedLobby, SteamMatchmaking.JoinLobby((CSteamID)lobbyId));
        }
    }

    private void OnJoinedLobby(LobbyEnter_t callback, bool error) {
        if (error || callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess) {
            PromptsController.OpenErrorPrompt("Failed to join the lobby (" + callback.m_ulSteamIDLobby + ")! Error code: " + (EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse);
            Debug.Log("Failed to join lobby! - " + (EChatRoomEnterResponse)callback.m_EChatRoomEnterResponse);
        }

        joiningLobby = false;

        NetworkingManager.CurrentLobby = callback.m_ulSteamIDLobby;
        if (NetworkingManager.LobbyJoined != null) NetworkingManager.LobbyJoined(callback.m_ulSteamIDLobby);
    }

    private void Update() {
        if (!LobbyBrowserManager.Lobbies.Contains(lobbyId)) {
            Destroy(gameObject);
        }
    }
}
