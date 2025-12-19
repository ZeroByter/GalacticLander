using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Steamworks;
using System.Collections;
using System.IO;

public class LevelSelectionController : MonoBehaviour, IPointerEnterHandler {
    [Header("The main level selection manager")]
    public LevelSelectionManager levelSelectionManager;

    [Header("The level name text and subtext")]
    public TMP_Text coopLevelText;
    public TMP_Text levelNameText;
    public TMP_Text subText;

    [Header("The preview image for this level")]
    public Image previewImage;

    [Header("The main canvas group")]
    public CanvasGroup mainCanvasGroup;

    [Header("The prefix for the level name")]
    public string levelDirPrefix = "";

    [Header("Is this a coop level?")]
    public bool coopLevel;

    [Header("The voting badge for this level")]
    public CanvasGroup votedBadgeGroup;
    public Image votedBadgeImage;
    public TMP_Text votedBadgeText;

    [HideInInspector]
    public LevelLoader.LevelOrigin origin;
    [HideInInspector]
    public string levelDirectory;

    private ulong workshopOwnerId;
    private string workshopName;

    private bool player1Voted;
    private bool player2Voted;

    private void OnEnable() {
        if (SteamManager.Initialized) {
            SteamCallbacks.LobbyChatUpdate_t.RegisterCallback(LobbyMemberDataUpdated);
            SteamCallbacks.LobbyDataUpdate_t.RegisterCallback(LobbyDataUpdated);
            SteamCallbacks.PersonaStateChange_t.RegisterCallback(OnPersonaStateChanged);
        }

        UpdateUI();
    }

    private void OnDestroy() {
        SteamCallbacks.LobbyChatUpdate_t.UnregisterCallback(LobbyMemberDataUpdated);
        SteamCallbacks.LobbyDataUpdate_t.UnregisterCallback(LobbyDataUpdated);
        SteamCallbacks.PersonaStateChange_t.UnregisterCallback(OnPersonaStateChanged);
    }

    private IEnumerator LoadPreviewFromFile(string path) {
        byte[] rawPreview = File.ReadAllBytes(path);
        Texture2D previewTexture = new Texture2D(0, 0);
        previewTexture.LoadImage(rawPreview);
        yield return previewTexture;
        Sprite previewSprite = Sprite.Create(previewTexture, new Rect(0, 0, previewTexture.width, previewTexture.height), new Vector2(0.5f, 0.5f));
        yield return previewSprite;

        previewImage.sprite = previewSprite;

        yield return null;
    }

	//`levelDirectory` is the file name/source of the level
	//creator id is the dude who made the level
    public void Setup(string levelDirectory, ulong creatorId = 0) {
        this.workshopOwnerId = 0;
        this.levelDirectory = levelDirectory;
        previewImage.color = Color.white;

        gameObject.SetActive(true);

        if (coopLevelText != null) coopLevelText.gameObject.SetActive(false);

        int levelNumber;
        if (int.TryParse(levelDirectory, out levelNumber)) { //this is an official game level
            levelNameText.text = "Level #" + levelDirectory;
            origin = LevelLoader.LevelOrigin.Game;

            Sprite levelPreview = Resources.Load<Sprite>(levelDirPrefix + levelDirectory);
            if(levelPreview != null && previewImage != null) {
                previewImage.sprite = levelPreview;
            } else {
                previewImage.sprite = null;
            }
        } else {
            if (levelDirectory.StartsWith("workshop:")) { //this is a workshop level
                ulong workshopId = ulong.Parse(levelDirectory.Replace("workshop:", ""));

                levelNameText.text = workshopId.ToString();

                mainCanvasGroup.alpha = 0;
                GetWorkshopLevelName(workshopId);
            } else {
                FileInfo fileInfo = new FileInfo(levelDirectory);
                if (fileInfo.Exists)
                {
                    levelNameText.text = fileInfo.Name.Replace(".level", "");
                    workshopName = "";
                }
                else
                {
                    Destroy(gameObject);
                }
            }
            origin = LevelLoader.LevelOrigin.External;
        }
    }

    private void GetWorkshopLevelName(ulong id) {
        UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[] { new PublishedFileId_t(id) }, 1);
        SteamUGC.SetReturnMetadata(handle, true);
        SteamUGC.SetReturnAdditionalPreviews(handle, false);
        SteamUGC.SetReturnChildren(handle, false);
        SteamUGC.SetAllowCachedResponse(handle, 3);
        SteamAPICall_t apiCall = SteamUGC.SendQueryUGCRequest(handle);
        SteamCallbacks.SteamUGCQueryCompleted_t.RegisterCallResult(SteamUGCQueryCompleted, apiCall);
    }

    IEnumerator GetWorkshopPreview(string url) {
        WWW www = new WWW(url);
        yield return www;
        Texture2D rawPreview = www.texture;
        Sprite previewSprite = Sprite.Create(rawPreview, new Rect(0, 0, rawPreview.width, rawPreview.height), new Vector2(0.5f, 0.5f));
        previewImage.sprite = previewSprite;
        previewImage.preserveAspect = true;
        
        previewImage.color = new Color(0.7075472f, 0.7075472f, 0.7075472f);
    }

    private void SteamUGCQueryCompleted(SteamUGCQueryCompleted_t callback, bool error) {
        if (callback.m_eResult != EResult.k_EResultOK) {
            SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
            throw new System.Exception("Got UGC query result with " + callback.m_eResult.ToString());
        }

        for (int i = 0; i < callback.m_unNumResultsReturned; i++) {
            SteamUGCDetails_t details;
            if (SteamUGC.GetQueryUGCResult(callback.m_handle, (uint)i, out details)) {
                if (details.m_eResult == EResult.k_EResultFileNotFound) {
                    Destroy(gameObject);
                } else {
                    if(coopLevelText != null) {
                        coopLevelText.gameObject.SetActive(levelDirPrefix == "sp" && !NetworkingManager.CurrentLobbyValid && details.m_rgchTags == "Co-op");
                    }

                    if (levelDirPrefix == "mp" && details.m_rgchTags != "Co-op") { //if our controller prefix is 'mp' and the level is not a coop level we...
                        Destroy(gameObject); //destroy the game gameobject
                        return; //halt the method
                    }

                    string previewUrl;
                    if (SteamUGC.GetQueryUGCPreviewURL(callback.m_handle, (uint)i, out previewUrl, 1024)) {
                        if(gameObject != null) StartCoroutine(GetWorkshopPreview(previewUrl));
                    } else {
                        previewImage.color = Color.white;
                    }

                    workshopOwnerId = details.m_ulSteamIDOwner;
                    if (!SteamFriends.RequestUserInformation((CSteamID)details.m_ulSteamIDOwner, true)) {
                        subText.text = "Created by: " + SteamFriends.GetFriendPersonaName((CSteamID)details.m_ulSteamIDOwner);
                    }
                    
                    mainCanvasGroup.alpha = 1;
                    workshopName = details.m_rgchTitle;
                    levelNameText.text = details.m_rgchTitle; //name of the level
                }
            }
        }

        SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
    }

    private void OnPersonaStateChanged(PersonaStateChange_t callback) {
        if(workshopOwnerId != 0) {
            subText.text = "Created by: " + SteamFriends.GetFriendPersonaName((CSteamID)workshopOwnerId);
        }
    }

    public void SelectLevel() {
        if (coopLevel) {
            if (origin == LevelLoader.LevelOrigin.Game) {
                SteamMatchmaking.SetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, "levelvoted", levelDirPrefix + levelDirectory);
                SteamMatchmaking.SetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, "levelvotedworkshopname", "");
            } else {
                SteamMatchmaking.SetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, "levelvoted", levelDirectory.Replace("workshop:", ""));
                SteamMatchmaking.SetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, "levelvotedworkshopname", workshopName);

                EItemState itemState = (EItemState)SteamUGC.GetItemState(new PublishedFileId_t(ulong.Parse(levelDirectory.Replace("workshop:", ""))));
                if (!itemState.HasFlag(EItemState.k_EItemStateSubscribed)) { //if we are not subscribed to this level we just voted for, begin downloading
                    SteamUGC.DownloadItem(new PublishedFileId_t(ulong.Parse(levelDirectory.Replace("workshop:", ""))), true);
                }
            }
        } else {
            if(origin == LevelLoader.LevelOrigin.Game) {
                LevelLoader.SetLevelDirectory(origin, levelDirPrefix + levelDirectory);
            } else {
                if (levelDirectory.StartsWith("workshop:")) { //this is a workshop level
                    ulong workshopId = ulong.Parse(levelDirectory.Replace("workshop:", ""));

                    ulong sizeOnDisk;
                    string path;
                    uint timestamp;
                    if(SteamUGC.GetItemInstallInfo(new PublishedFileId_t(workshopId), out sizeOnDisk, out path, 2000, out timestamp)) {
                        LevelLoader.SetLevelDirectory(origin, path + "/" + workshopId.ToString() + ".level");
                    }
                } else {
                    LevelLoader.SetLevelDirectory(origin, levelDirectory);
                }
            }
            SceneManager.LoadScene("Game Level");
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (origin == LevelLoader.LevelOrigin.Game) {
            levelSelectionManager.ShowLevelDetails(levelDirPrefix + levelDirectory);
        } else {
            if (!levelDirectory.StartsWith("workshop:"))
            {
                FileInfo fileInfo = new FileInfo(levelDirectory);
                if (fileInfo.Exists)
                {
                    string previewPath = levelDirectory.Replace(".level", ".png");
                    if (File.Exists(previewPath))
                    {
                        StartCoroutine(LoadPreviewFromFile(previewPath));
                    }
                }
            }

            levelSelectionManager.ShowLevelDetails(levelDirectory);
        }
    }

    private void Update() {
        if (!coopLevel) return;

        if(player1Voted || player2Voted) {
            votedBadgeGroup.alpha = Mathf.Lerp(votedBadgeGroup.alpha, 1, 0.5f);
        } else {
            votedBadgeGroup.alpha = Mathf.Lerp(votedBadgeGroup.alpha, 0, 0.3f);
        }

        if (player1Voted && player2Voted) {
            votedBadgeImage.color = Color.Lerp(votedBadgeImage.color, Color.green, 0.35f);
            votedBadgeText.text = "Both players voted!";
        } else {
            votedBadgeImage.color = Color.Lerp(votedBadgeImage.color, Color.white, 0.5f);
            if (player1Voted) { //if only one player has voted
                votedBadgeText.text = "Player #1's choice";
            } else if (player2Voted) { //if only player 2 has voted
                votedBadgeText.text = "Player #2's choice";
            } else { //if no player has voted
                
            }
        }
    }

    private void UpdateUI() {
        if (!coopLevel) return;

        ulong player1 = (ulong) SteamMatchmaking.GetLobbyOwner((CSteamID) NetworkingManager.CurrentLobby);
        ulong player2 = LobbyMenuController.GetNonOwnerPlayer();

        string levelName = levelDirPrefix + levelDirectory;

        if (origin != LevelLoader.LevelOrigin.Game) levelName = levelDirectory.Replace("workshop:", "");

        player1Voted = SteamMatchmaking.GetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, (CSteamID) player1, "levelvoted") == levelName;
        if (player2 != 0) {
            string player2LevelVoted = SteamMatchmaking.GetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, (CSteamID)player2, "levelvoted");
            player2Voted = player2LevelVoted == levelName;
        } else {
            player2Voted = false;
        }
    }

    private void LobbyMemberDataUpdated(LobbyChatUpdate_t callback) {
        UpdateUI();
    }

    private void LobbyDataUpdated(LobbyDataUpdate_t callback) {
        UpdateUI();
    }
}
