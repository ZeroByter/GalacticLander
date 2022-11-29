using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using Steamworks;
using System.IO;

public class LevelSelectionManager : MonoBehaviour {
    public static int TotalSingleplayerLevels = 0;
    public static int TotalCoopLevels = 0;

    [Header("Number of levels")]
    public int levels;

    [Header("Templates")]
    public LevelSelectionController unknownVoteTemplate;
    public LevelSelectionController template;

    [Header("Selected level information section")]
    public TMP_Text levelDetails;

    [Header("Gravity & no-gravity mode checkmarks")]
    public LerpImageColor gravitySelectedCheckmark;
    public LerpImageColor noGravitySelectedCheckmark;
    public Color gravityModeSelectedColor = Color.white;
    /// <summary>
    /// The color to display when the other player has voted for this particular mode, but we haven't
    /// </summary>
    public Color gravityModeOtherSelectedColor = Color.white;

    [Header("Toggle view legacy levels button")]
    public GameObject viewLegacyLevelsButton;
    public GameObject viewDefaultLevelsButton;

    private bool legacyLevelsLastViewedSingleplayer = true;
    private string levelEditorLevelsPath;

    private void OnEnable() {
        if (SteamManager.Initialized) {
            SteamCallbacks.LobbyChatUpdate_t.RegisterCallback(LobbyMemberDataUpdated);
            SteamCallbacks.LobbyDataUpdate_t.RegisterCallback(LobbyDataUpdated);
        }

        CheckUnknownWorkshopLevelVote();
    }

    private void OnDestroy() {
        SteamCallbacks.LobbyChatUpdate_t.UnregisterCallback(LobbyMemberDataUpdated);
        SteamCallbacks.LobbyDataUpdate_t.UnregisterCallback(LobbyDataUpdated);
    }

    private void Awake() {
        template.gameObject.SetActive(false);
        levelEditorLevelsPath = Application.persistentDataPath + "/Level Editor/Levels/";

        if (template.coopLevel) {
            TotalCoopLevels = levels;
        } else {
            TotalSingleplayerLevels = levels;
        }

        ShowDefaultList();
    }

    private void Start() {
        UpdateGravityVoteUI();
    }

    private void ShowViewLegacyLevels(bool show)
    {
        viewLegacyLevelsButton.SetActive(show);
        viewDefaultLevelsButton.SetActive(!show);
    }

    private void HideLegacyLevelsButtons()
    {
        viewLegacyLevelsButton.SetActive(false);
        viewDefaultLevelsButton.SetActive(!false);
    }

    public void ShowDefaultList()
    {
        ClearLevelsList();

        for (int i = 0; i < 30; i++)
        {
            CreateLevelTemplate((i + 1).ToString(), false, false);
        }

        legacyLevelsLastViewedSingleplayer = true;
        ShowViewLegacyLevels(true);
        MoveLegacyButtonsToEndOfList();
    }

    public void ShowLegacyDefaultList()
    {
        ClearLevelsList();

        for (int i = 0; i < 20; i++)
        {
            CreateLevelTemplate((i + 1).ToString(), false, true);
        }

        legacyLevelsLastViewedSingleplayer = true;
        ShowViewLegacyLevels(false);
        MoveLegacyButtonsToEndOfList();
    }

    public void ShowDefaultOverrideCoopList()
    {
        ClearLevelsList();

        for (int i = 0; i < 30; i++)
        {
            CreateLevelTemplate((i + 1).ToString(), true, false);
        }

        legacyLevelsLastViewedSingleplayer = false;
        ShowViewLegacyLevels(true);
        MoveLegacyButtonsToEndOfList();
    }

    public void ShowDefaultOverrideCoopLegacyList()
    {
        ClearLevelsList();

        for (int i = 0; i < 20; i++)
        {
            CreateLevelTemplate((i + 1).ToString(), true, true);
        }

        legacyLevelsLastViewedSingleplayer = false;
        ShowViewLegacyLevels(false);
        MoveLegacyButtonsToEndOfList();
    }

    public void ShowRegularsLevelsList()
    {
        if (legacyLevelsLastViewedSingleplayer)
        {
            ShowDefaultList();
        }
        else
        {
            ShowDefaultOverrideCoopList();
        }
    }

    public void ShowLegacyLevelsList()
    {
        if (legacyLevelsLastViewedSingleplayer)
        {
            ShowLegacyDefaultList();
        }
        else
        {
            ShowDefaultOverrideCoopLegacyList();
        }
    }

    private void MoveLegacyButtonsToEndOfList()
    {
        var itemCount = template.transform.parent.childCount;

        viewDefaultLevelsButton.transform.SetSiblingIndex(itemCount - 1);
        viewLegacyLevelsButton.transform.SetSiblingIndex(itemCount - 2);
    }

    public void ShowLevelEditorList() {
        ClearLevelsList();

        if (Directory.Exists(levelEditorLevelsPath)) {
            DirectoryInfo folderInfo = new DirectoryInfo(levelEditorLevelsPath);
            foreach(FileInfo file in folderInfo.GetFiles()) {
                if(file.Extension == ".level") {
                    CreateLevelTemplate(file.FullName, false, false);
                }
            }
        }

        MoveLegacyButtonsToEndOfList();
        HideLegacyLevelsButtons();
    }

    public void ShowWorkshopList() {
        ClearLevelsList();

        PublishedFileId_t[] subscribedItems = new PublishedFileId_t[SteamUGC.GetNumSubscribedItems()];
        SteamUGC.GetSubscribedItems(subscribedItems, (uint)subscribedItems.Length);

        foreach (PublishedFileId_t file in subscribedItems) {
            EItemState itemState = (EItemState)SteamUGC.GetItemState(file);

            if (itemState.HasFlag(EItemState.k_EItemStateInstalled)) {
                CreateLevelTemplate("workshop:" + file.m_PublishedFileId.ToString(), false, false);
            }
        }

        MoveLegacyButtonsToEndOfList();
        HideLegacyLevelsButtons();
    }

    public void ClearLevelsList() {
        var itemCount = template.transform.parent.childCount;
        var index = -1;

        foreach(Transform template in template.transform.parent) {
            index++;
            if (this.template.coopLevel && template.GetSiblingIndex() == 0) continue; //dont delete the first element which is intended for when the other player votes for a level we are not subscribed to
            if (index > itemCount - 3) continue;
            if (template.gameObject.activeSelf) Destroy(template.gameObject); //only delete active templates
        }
    }

    private void CreateLevelTemplate(string levelName, bool coop, bool legacy) {
        LevelSelectionController controller = Instantiate(template, template.transform.parent).GetComponent<LevelSelectionController>();
        if (coop) controller.levelDirPrefix = "mp";
        if (legacy) controller.levelDirPrefix = "o" + controller.levelDirPrefix;
        controller.Setup(levelName);
    }

    public void ShowLevelDetails(string levelName) {
        string levelDisplayName = "";
        
        if(levelName.StartsWith("sp") || levelName.StartsWith("mp")) { //if the 'levelName' (which isn't always really a name) starts with either sp or mp
            levelDisplayName = "Level #" + int.Parse(levelName.Replace("sp", "").Replace("mp", ""));

            ShowLevelProgress(levelName, levelDisplayName, LevelProgress.LevelType.Game);
        } else {
            if (levelName.StartsWith("workshop:")) {
                UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[] { new PublishedFileId_t(ulong.Parse(levelName.Replace("workshop:", ""))) }, 1);
                SteamUGC.SetReturnMetadata(handle, false);
                SteamUGC.SetReturnAdditionalPreviews(handle, false);
                SteamUGC.SetReturnChildren(handle, false);
                SteamUGC.SetAllowCachedResponse(handle, 4);
                SteamAPICall_t apiCall = SteamUGC.SendQueryUGCRequest(handle);
                SteamCallbacks.SteamUGCQueryCompleted_t.RegisterCallResult(SteamUGCQueryCompleted, apiCall);
            } else {
                levelDisplayName = new FileInfo(levelName).Name.Replace(".level", "");

                ShowLevelProgress(levelName, levelDisplayName, LevelProgress.LevelType.External);
            }
        }
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
                    ShowLevelProgress("workshop:" + details.m_nPublishedFileId.m_PublishedFileId, details.m_rgchTitle, LevelProgress.LevelType.External);
                }
            }
        }

        SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
    }

    private void ShowLevelProgress(string levelName, string levelDisplayName, LevelProgress.LevelType type) {
        LevelProgress progress = LevelProgressCounter.GetProgress(type, levelName, LevelLoader.GravityEnabled);

        if(progress != null) {
            //print(string.Format("name = {0} - times completed = {1}", levelName, progress.timesCompleted));
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>" + levelDisplayName + "</b>");

        if (progress != null && progress.timesCompleted != 0) {
            sb.AppendLine();
            if(progress.timesCompleted == 1) {
                sb.AppendLine("You have completed this level " + progress.timesCompleted + " time!");
            } else {
                sb.AppendLine("You have completed this level " + progress.timesCompleted + " times!");
            }
            sb.AppendLine("It took you " + progress.triesBeforeSuccess + " attempts before you completed this level for the first time!");
            sb.AppendLine();
            sb.AppendLine("Last completion time: " + progress.lastPlayedTime);
            sb.AppendLine("Last completion score: " + progress.lastPlayedScore);
            sb.AppendLine();
            sb.AppendLine("Best completion time: " + progress.bestTime);
            sb.AppendLine("Best completion score: " + progress.bestScore);
        }

        levelDetails.text = sb.ToString();
    }

    public void OpenWorkshop() {
        SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/app/913600/workshop/");
    }

    private void CheckUnknownWorkshopLevelVote() {
        if (NetworkingManager.CurrentLobby == 0) {
            if(unknownVoteTemplate != null) unknownVoteTemplate.gameObject.SetActive(false);
            return;
        }
        if (unknownVoteTemplate == null) {
            if (unknownVoteTemplate != null) unknownVoteTemplate.gameObject.SetActive(false);
            return;
        }
        
        if (SteamMatchmaking.GetNumLobbyMembers(new CSteamID(NetworkingManager.CurrentLobby)) != 1) {
            ulong otherPlayer = LobbyMenuController.GetOtherPlayer();

            if(otherPlayer != 0) {
                string otherPlayerLevelVoted = SteamMatchmaking.GetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, (CSteamID)otherPlayer, "levelvoted");
                
                ulong otherPlayerWorkshopLevelVoted;
                if(ulong.TryParse(otherPlayerLevelVoted, out otherPlayerWorkshopLevelVoted)) {
                    EItemState itemState = (EItemState)SteamUGC.GetItemState(new PublishedFileId_t(otherPlayerWorkshopLevelVoted));

                    if (!itemState.HasFlag(EItemState.k_EItemStateSubscribed)) {
                        unknownVoteTemplate.gameObject.SetActive(true); //only enable if we are not subscribed
                        if(unknownVoteTemplate.levelDirectory != "workshop:" + otherPlayerWorkshopLevelVoted.ToString()) unknownVoteTemplate.Setup("workshop:" + otherPlayerWorkshopLevelVoted.ToString());
                    } else {
                        unknownVoteTemplate.gameObject.SetActive(false);
                    }
                } else {
                    unknownVoteTemplate.gameObject.SetActive(false);
                }
            }
        } else {
            unknownVoteTemplate.gameObject.SetActive(false);
        }
    }

    private void SelectGravityModeCheckbox(bool gravityEnabled) {
        if (gravityEnabled) {
            gravitySelectedCheckmark.target = gravityModeSelectedColor;
            noGravitySelectedCheckmark.target = new Color(0, 0, 0, 0);
        } else {
            gravitySelectedCheckmark.target = new Color(0, 0, 0, 0);
            noGravitySelectedCheckmark.target = gravityModeSelectedColor;
        }
    }

    private void SelectGravityModeCheckbox(bool localGravityVote, bool bothVotesMatch) {
        Color localVoteColorMismatch = gravityModeSelectedColor;
        localVoteColorMismatch.a = gravityModeOtherSelectedColor.a;

        if (bothVotesMatch) {
            if (localGravityVote) {
                gravitySelectedCheckmark.target = gravityModeSelectedColor;
                noGravitySelectedCheckmark.target = new Color(0, 0, 0, 0);
            } else {
                gravitySelectedCheckmark.target = new Color(0, 0, 0, 0);
                noGravitySelectedCheckmark.target = gravityModeSelectedColor;
            }
        } else {
            if (localGravityVote) {
                gravitySelectedCheckmark.target = localVoteColorMismatch;
                noGravitySelectedCheckmark.target = gravityModeOtherSelectedColor;
            } else {
                gravitySelectedCheckmark.target = gravityModeOtherSelectedColor;
                noGravitySelectedCheckmark.target = localVoteColorMismatch;
            }
        }
    }

    private void UpdateGravityVoteUI() {
        if (template.coopLevel && NetworkingManager.CurrentLobbyValid) {
            CSteamID lobbyId = (CSteamID)NetworkingManager.CurrentLobby;
            bool localVote = SteamMatchmaking.GetLobbyMemberData(lobbyId, SteamUser.GetSteamID(), "voteGravity") == "True";

            if(SteamMatchmaking.GetNumLobbyMembers(lobbyId) == 1) { //if we are the only players in the lobby
                SelectGravityModeCheckbox(localVote, true);
            } else { //if there are two people in the lobby
                bool otherPlayerVote = SteamMatchmaking.GetLobbyMemberData(lobbyId, (CSteamID) LobbyMenuController.GetOtherPlayer(), "voteGravity") == "True";
                SelectGravityModeCheckbox(localVote, localVote == otherPlayerVote);
            }
        } else {
            SelectGravityModeCheckbox(LevelLoader.GravityEnabled);
        }
    }

    private void LobbyMemberDataUpdated(LobbyChatUpdate_t callback) {
        CheckUnknownWorkshopLevelVote();

        if (template.coopLevel) UpdateGravityVoteUI();
    }

    private void LobbyDataUpdated(LobbyDataUpdate_t callback) {
        if (callback.m_ulSteamIDLobby != NetworkingManager.CurrentLobby) return;
        if (!template.coopLevel) return;

        CheckUnknownWorkshopLevelVote();

        if(template.coopLevel) UpdateGravityVoteUI();
    }

    public void SelectGravityMode() {
        LevelLoader.GravityEnabled = true;
        if (!template.coopLevel) UpdateGravityVoteUI();
    }

    public void SelectNoGravityMode() {
        LevelLoader.GravityEnabled = false;
        if (!template.coopLevel) UpdateGravityVoteUI();
    }
}
