using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;

public class ScreenshotsHooker : MonoBehaviour {
    private static ScreenshotsHooker Singletron;

    private void OnEnable() {
        if (SteamManager.Initialized) {
            SteamCallbacks.ScreenshotReady_t.RegisterCallback(OnScreenshotReady);
        }
    }

    private void OnDisable() {
        SteamCallbacks.ScreenshotReady_t.UnregisterCallback(OnScreenshotReady);
    }

    private void Awake() {
        if(Singletron != null) {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Singletron = this;
    }

    private void OnScreenshotReady(ScreenshotReady_t callback) {
        if(callback.m_eResult != EResult.k_EResultOK) throw new System.Exception("screenshot ready failure - " + callback.m_eResult.ToString());

        if (NetworkingManager.CurrentLobbyValid) { //if we are in a lobby
            ulong otherPlayer = LobbyMenuController.GetOtherPlayer();

            if(otherPlayer != 0) { //if there is another player with us
                SteamScreenshots.TagUser(callback.m_hLocal, (CSteamID)otherPlayer);
            }
        }

        string levelFileName = Path.GetFileNameWithoutExtension(LevelLoader.GetLevelDirectory());
        ulong levelWorkshopId;
        if(ulong.TryParse(levelFileName, out levelWorkshopId)) {
            SteamScreenshots.TagPublishedFile(callback.m_hLocal, new PublishedFileId_t(levelWorkshopId));
        }

        if(SceneManager.GetActiveScene().name == "Game Level") {
            string rawLevelName = LevelLoader.GetLevelDirectory();
            string realLevelName = "";

            if(rawLevelName.StartsWith("sp") || rawLevelName.StartsWith("mp")) {
                realLevelName = "Level #" + rawLevelName.Replace("sp", "").Replace("mp", "");
            } else {
                if(levelWorkshopId != 0) { //if this is a workshop level
                    CSteamID lobbyOwner = SteamMatchmaking.GetLobbyOwner((CSteamID)NetworkingManager.CurrentLobby);
                    realLevelName = SteamMatchmaking.GetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, lobbyOwner, "levelvotedworkshopname");
                }
            }

            if(realLevelName != "") {
                SteamScreenshots.SetLocation(callback.m_hLocal, realLevelName);
            }
        }
    }
}
