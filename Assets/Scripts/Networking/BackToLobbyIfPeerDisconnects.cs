using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;

public class BackToLobbyIfPeerDisconnects : MonoBehaviour {
    private void LobbyMemberDataUpdated(LobbyChatUpdate_t callback) {
        if(callback.m_ulSteamIDMakingChange != (ulong) SteamUser.GetSteamID() && (EChatMemberStateChange)callback.m_rgfChatMemberStateChange != EChatMemberStateChange.k_EChatMemberStateChangeEntered) {
            //TODO: assign static variable in either lobby manager or lobby chat manager to display text message that other player quit to lobby (hence thats why we got kicked too)
            SceneManager.LoadScene("Main Menu");
        }
    }

    private void Awake() {
        if(SteamManager.Initialized && NetworkingManager.CurrentLobbyValid) {
            SteamMatchmaking.SetLobbyMemberData((CSteamID) NetworkingManager.CurrentLobby, "isInGame", "true");
        }
    }

    private void OnEnable() {
        if (SteamManager.Initialized) {
            SteamCallbacks.LobbyChatUpdate_t.RegisterCallback(LobbyMemberDataUpdated);
        }
    }

    private void OnDisable() {
        SteamCallbacks.LobbyChatUpdate_t.UnregisterCallback(LobbyMemberDataUpdated);
    }

    //do static method to send packet like below...
    public static void SendPeerBackToLobbyPacket() {
        if (SteamManager.Initialized && NetworkingManager.CurrentLobbyValid) {
            NetworkingManager.SendPacketOtherOnly(new object[] { 5 }, 1, EP2PSend.k_EP2PSendReliable);
        }
    }

    public void SendPacket() {
        SendPeerBackToLobbyPacket();
    }
}
