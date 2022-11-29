using System.IO;
using UnityEngine;
using TMPro;
using Steamworks;

public class LevelNameIntro : MonoBehaviour {
    private TMP_Text text;
    private TMP_Text outlineText;

    private string workshopItemTitle;
    
    private void Awake() {
        text = GetComponent<TMP_Text>();
        outlineText = transform.parent.gameObject.GetComponent<TMP_Text>();
    }

    private void Start() {
        if (LevelLoader.PlayTestingLevel)
        {
            text.text = "";
            return;
        }

        string levelNiceName = "";
        string levelName = LevelLoader.GetLevelDirectory();

        if (LevelLoader.IsPlayingEventLevel()) {
            levelNiceName = "Level #" + (LevelLoader.Singletron.GetCurrentEventLevelNumber() + 1);
        } else {
            if (levelName.StartsWith("sp") || levelName.StartsWith("mp")) { //in-game level
                int levelNumber = int.Parse(levelName.Replace("sp", "").Replace("mp", ""));
                levelNiceName = "Level #" + levelNumber;
            } else if (levelName.EndsWith(".level")) { //external level
                string fileName = Path.GetFileNameWithoutExtension(levelName);
                ulong workshopId;

                if (ulong.TryParse(fileName, out workshopId)) {
                    UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[] { new PublishedFileId_t(workshopId) }, 1);
                    SteamUGC.SetReturnMetadata(handle, false);
                    SteamUGC.SetReturnAdditionalPreviews(handle, false);
                    SteamUGC.SetReturnChildren(handle, false);
                    SteamUGC.SetAllowCachedResponse(handle, 4);
                    SteamAPICall_t apiCall = SteamUGC.SendQueryUGCRequest(handle);
                    SteamCallbacks.SteamUGCQueryCompleted_t.RegisterCallResult(SteamUGCQueryCompleted, apiCall);
                    levelNiceName = "";
                } else {
                    levelNiceName = fileName;
                }
            }
        }

        text.text = levelNiceName;
    }

    private void SteamUGCQueryCompleted(SteamUGCQueryCompleted_t callback, bool error) {
        if (callback.m_eResult != EResult.k_EResultOK) {
            SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
            throw new System.Exception("Got UGC query result with " + callback.m_eResult.ToString());
        }

        for (int i = 0; i < callback.m_unNumResultsReturned; i++) {
            SteamUGCDetails_t details;
            if (SteamUGC.GetQueryUGCResult(callback.m_handle, (uint)i, out details)) {
                if (details.m_eResult == EResult.k_EResultOK) {
                    workshopItemTitle = details.m_rgchTitle;

                    if (!SteamFriends.RequestUserInformation((CSteamID)details.m_ulSteamIDOwner, true)) {
                        text.text = details.m_rgchTitle + "\nCreated by: \"" + SteamFriends.GetFriendPersonaName((CSteamID)details.m_ulSteamIDOwner) + "\"";
                    }
                }
            }
        }

        SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
    }

    private void Update() {
        if (text.text == "") return;

        float lerpSpeed = 1.25f * Time.deltaTime;

        text.fontSize = Mathf.Lerp(text.fontSize, 0, lerpSpeed);
        text.color = Color.Lerp(text.color, new Color(1, 1, 1, 0), lerpSpeed);
        
        if (text.fontSize < 10) {
            text.color = new Color(0, 0, 0, 0);
        }

        Color outlineColor = outlineText.color;
        outlineColor.a = text.color.a;
        outlineText.color = outlineColor;
    }
}
