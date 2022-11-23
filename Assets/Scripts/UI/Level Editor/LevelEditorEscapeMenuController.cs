using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Steamworks;
using TMPro;
using System.Text;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System;

public class LevelEditorEscapeMenuController : MonoBehaviour {
    public static LevelEditorEscapeMenuController Singletron;

    [Header("The transition")]
    public CanvasBlurTransition transition;

    [Header("Workshop details hider")]
    public CanvasGroup workshopCanvasGroup;

    [Header("The header Steam Workshop logo")]
    public Image steamWorkshopLogo;
    public Sprite regularWorkshopLogo;
    public Sprite viewOnWorkshopLogo;

    [Header("TMPro stuff")]
    public TMP_Text conditionsText;
    public TMP_InputField nameField;
    public TMP_InputField descField;

    [Header("Level preview background images")]
    public Image mainEscapeMenuBackground;
    public Image choosePreviewDefaultImage;

    [Header("Level preview prompt")]
    public LerpCanvasGroup choosePreviewLerpGroup;
    public Image customPreviewImage;

    [Header("Workshop upload prompt")]
    public LerpCanvasGroup uploadPromptLerpGroup;
    public Slider uploadPromptSlider;

    [Header("Choose level theme prompt")]
    public LerpCanvasGroup chooseLevelThemePrompt;

    [Header("Other stuff")]
    public Button playtestButton;
    public Button workshopSubmitButton;
    public RectTransform saveLevelButtons;

    private bool hideWorkshopDetails = false;
    private List<bool> allConditions;

    private float lastCheckedUpdateUploadProgress;

    private FileSystemWatcher watcher;

    private void Awake() {
        Singletron = this;

        choosePreviewLerpGroup.ForceAlpha(0);
        uploadPromptLerpGroup.ForceAlpha(0);
        chooseLevelThemePrompt.ForceAlpha(0);

        watcher = new FileSystemWatcher();
        watcher.Path = Environment.CurrentDirectory;
        watcher.Changed += OnCustomPreviewPathChanged;
        watcher.EnableRaisingEvents = true;
    }

    private void OnDestroy() {
        watcher.Changed -= OnCustomPreviewPathChanged;
        watcher.EnableRaisingEvents = false;
    }

    private void OnCustomPreviewPathChanged(object source, FileSystemEventArgs e) {
        if(e.Name.EndsWith("CustomPreviewImagePath.dat")) {
            string[] fileLines = File.ReadAllLines(e.FullPath);

            if (fileLines.Length == 1) {
                FileInfo newPreviewInfo = new FileInfo(fileLines[0]);

                if (newPreviewInfo.Exists && newPreviewInfo.Length < 1024 * 1024) { //if the file really exists and is less than a megabyte
                    string[] imageExtensions = new string[] { ".jpg", ".jpeg", ".gif", ".png" };
                    bool isFileImage = imageExtensions.Contains(newPreviewInfo.Extension);

                    if (isFileImage) { //if the file is really an image
                        overridePreviewPath = fileLines[0];
                        CloseChoosePreviewPrompt();

                        Texture2D previewTexture = new Texture2D(2, 2);
                        byte[] dataBytes = File.ReadAllBytes(overridePreviewPath);
                        previewTexture.LoadImage(dataBytes);
                        Sprite previewSprite = Sprite.Create(previewTexture, new Rect(0, 0, previewTexture.width, previewTexture.height), new Vector2(0.5f, 0.5f));
                        customPreviewImage.color = new Color(1, 1, 1, 1);
                        customPreviewImage.sprite = previewSprite;
                        customPreviewImage.preserveAspect = true;
                    }
                }
            }
        }
    }

    private void Update() {
        if(Time.time > lastCheckedUpdateUploadProgress + 0.1f) {
            lastCheckedUpdateUploadProgress = Time.time;

            if(currentUpdateHandle.m_UGCUpdateHandle != 0) {
                ulong processed;
                ulong total;
                SteamUGC.GetItemUpdateProgress(currentUpdateHandle, out processed, out total);

                uploadPromptLerpGroup.target = total == 0 ? 0 : 1;
                if (total != 0) {
                    uploadPromptSlider.value = processed / total;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && LastPressedEscape.LastPressedEscapeCooldownOver(0.1f)) {
            LastPressedEscape.SetPressedEscape();

            if(chooseLevelThemePrompt.currentAlpha > 0.5f) {
                CloseLevelThemePrompt();
            } else {
                transition.ToggleMenu();

                if (transition.isOpen) { //if we just opened the menu...
                    CheckConditions(); //update conditions

                    LevelData currentLevelData = LevelEditorManager.GetLevelData();
                    if (currentLevelData != null) {
                        steamWorkshopLogo.GetComponent<Button>().interactable = currentLevelData.workshopId != 0;
                        if (currentLevelData.workshopId == 0) {
                            steamWorkshopLogo.sprite = regularWorkshopLogo;
                        } else {
                            steamWorkshopLogo.sprite = viewOnWorkshopLogo;
                        }
                    }
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(saveLevelButtons);
        }

        workshopCanvasGroup.alpha = Mathf.Lerp(workshopCanvasGroup.alpha, hideWorkshopDetails ? 0.8f : 1, 0.3f);
    }

    public void ShowWorkshopItem() {
        LevelData currentLevelData = LevelEditorManager.GetLevelData();
        if (currentLevelData == null || currentLevelData.workshopId == 0) return;

        SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/filedetails/?id=" + currentLevelData.workshopId);
    }

    public void ShowWorkshopCanvasGroup() {
        hideWorkshopDetails = false;
    }

    public void HideWorkshopCanvasGroup() {
        hideWorkshopDetails = true;
    }

    private void AddCondition(string text, bool isFullfilled) {
        string textToAdd = text;
        if (isFullfilled) {
            textToAdd = "<sprite=0><color=#19B024> " + text + "</color>";
        } else {
            textToAdd = "<sprite=1> " + text;
        }
        conditionsText.text = conditionsText.text + textToAdd + "\n\n";

        allConditions.Add(isFullfilled);
    }

    private int[] countPads() {
        int[] counts = new int[2];

        foreach (LevelObject obj in LevelEditorManager.GetLevelData().levelData) {
            if (obj.GetType() == typeof(LevelEntity)) {
                LevelEntity ent = (LevelEntity)obj;
                if (ent.resourceName == "Ship Pads/Launch Pad") {
                    counts[0]++;
                }
                if (ent.resourceName == "Ship Pads/Land Pad") {
                    counts[1]++;
                }
            }
        }

        return counts;
    }

    private bool AreAllTilesValid() {
        if (LevelEditorManager.GetLevelData().GetTiles().Count == 0) return false;

        foreach(LevelObject levelObject in LevelEditorManager.GetLevelData().levelData) {
            if(levelObject.GetType() == typeof(LevelTile)) {
                LevelTile tile = (LevelTile)levelObject;

                if (tile.spriteName == "Filled Tile") return false;
            }
        }

        return true;
    }

    private void CheckConditions() {
        conditionsText.text = "";

        allConditions = new List<bool>();
        LevelData data = LevelEditorManager.GetLevelData();
        data.SortTilesList();
        data.SortLines();

        var offset = new Vector2(18, 18);

        bool launchPadsInsideLevel = true;
        bool landingPadsInsideLevel = true;
        foreach (LevelObject obj in data.levelData) {
            if(obj.GetType() == typeof(LevelEntity)) {
                LevelEntity ent = (LevelEntity)obj;

                if (ent.resourceName == "Ship Pads/Launch Pad" || ent.resourceName == "Ship Pads/Land Pad")
                {
                    var upDirection = new Vector2(Mathf.Sin((-ent.rotation) * Mathf.Deg2Rad), Mathf.Cos((-ent.rotation) * Mathf.Deg2Rad));
                    var rightDirection = new Vector2(Mathf.Sin((-ent.rotation + 90) * Mathf.Deg2Rad), Mathf.Cos((-ent.rotation + 90) * Mathf.Deg2Rad));

                    var leftBottomCorner = ent.GetPosition() - rightDirection * 0.725f + upDirection * 0.175f + offset;
                    var rightBottomCorner = ent.GetPosition() + rightDirection * 0.725f + upDirection * 0.175f + offset;
                    var leftTopCorner = ent.GetPosition() - rightDirection * 0.725f + upDirection * 0.525f + offset;
                    var rightTopCorner = ent.GetPosition() + rightDirection * 0.725f + upDirection * 0.525f + offset;

                    if (ent.resourceName == "Ship Pads/Launch Pad")
                    {
                        if (data.IsPointInLevelNew(leftBottomCorner) || data.IsPointInLevelNew(rightBottomCorner) || data.IsPointInLevelNew(leftTopCorner) || data.IsPointInLevelNew(rightTopCorner))
                        {
                            launchPadsInsideLevel = false;
                        }
                    }
                    if (ent.resourceName == "Ship Pads/Land Pad")
                    {
                        if (data.IsPointInLevelNew(leftBottomCorner) || data.IsPointInLevelNew(rightBottomCorner) || data.IsPointInLevelNew(leftTopCorner) || data.IsPointInLevelNew(rightTopCorner))
                        {
                            landingPadsInsideLevel = false;
                        }
                    }
                }
            }
        }
        
        AddCondition("All launch pads must be completely inside the level", launchPadsInsideLevel);
        AddCondition("All land pads must be completely inside the level", landingPadsInsideLevel);

        workshopSubmitButton.interactable = !allConditions.Contains(false);
        playtestButton.interactable = workshopSubmitButton.interactable;
    }

    public void BackToMainMenu() {
        CursorController.RemoveUser("editorEraser");
        SceneManager.LoadScene("Main Menu");
    }

    public static bool IsOpen() {
        return Singletron.transition.isOpen;
    }

    public void CheckIfShouldBeOpen() {
        if(CursorController.GetUser("LoadSavedLevelsMenu") != null) {
            transition.CloseMenu();
        }
    }

    private void _UploadWorkshopItem(bool curator) {
        if (!SteamManager.Initialized) return; //show error that says steam is not initalized

        if(nameField.text.Length < 4) {
            PromptsController.OpenErrorPrompt("Workshop item title is less than 4 charachters");
            return;
        }

        if (descField.text.Length < 10) {
            PromptsController.OpenErrorPrompt("Workshop item description is less than 10 charachters");
            return;
        }

        Directory.CreateDirectory(Application.persistentDataPath + "/Level Editor/Temporary Workshop Upload/");

        //delete all files that may be inside the folder we are about to upload
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath + "/Level Editor/Temporary Workshop Upload/");
        foreach (FileInfo file in directoryInfo.GetFiles()) {
            file.Delete();
        }

        LevelData currentLevelData = LevelEditorManager.GetLevelData();
        if (currentLevelData.workshopId != 0) {
            StartItemUpdate(new PublishedFileId_t(currentLevelData.workshopId), false);
            originalPublish = false;
        } else {
            SteamAPICall_t apiCall;

            if (curator)
            {
                apiCall = SteamUGC.CreateItem(new AppId_t((uint)SteamManager.AppId), EWorkshopFileType.k_EWorkshopFileTypeConcept); //for curated workshop
            }
            else
            {
                apiCall = SteamUGC.CreateItem(new AppId_t((uint)SteamManager.AppId), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
            }
            
            SteamCallbacks.CreateItemResult_t.RegisterCallResult(WorkshopItemCreated, apiCall);
            originalPublish = true;
        }
    }

    public void UploadWorkshopItem()
    {
        _UploadWorkshopItem(false);
    }

    public void UploadCuratorWorkshopItemConfirm()
    {
        _UploadWorkshopItem(true);
    }

    /// <summary>
    /// Whether or not this is the first time we are uploading/updating this workshop item
    /// </summary>
    private bool originalPublish;
    public UGCUpdateHandle_t currentUpdateHandle;
    /// <summary>
    /// For when we are using a custom preview image
    /// </summary>
    private string overridePreviewPath;

    private void StartItemUpdate(PublishedFileId_t fileId, bool needsToAcceptWorkshopLegalAgreement) {
        //set the level data workshop steam id
        LevelEditorManager.GetLevelData().workshopId = ((ulong)fileId);

        //setup some path strings
        string folderPath = Application.persistentDataPath + "/Level Editor/Temporary Workshop Upload/";
        string filePath = folderPath + fileId + ".level";

        bool usingOverridePreview = false;
        string previewPath = folderPath + fileId + ".png";

        //if the override preview path is set, we check if it's valid and use that instead
        if (!string.IsNullOrEmpty(overridePreviewPath)) {
            FileInfo previewFile = new FileInfo(overridePreviewPath);
            if (previewFile.Exists) { //if the file exists
                if(previewFile.Length < 1024 * 1024) { //if file is less than a megabyte
                    usingOverridePreview = true;
                } else {
                    PromptsController.OpenErrorPrompt("Preview image file is larger than one megabyte!");
                    return;
                }
            } else {
                PromptsController.OpenErrorPrompt("Invalid file path!");
                return;
            }
        }

        //begin to setup workshop item variables
        currentUpdateHandle = SteamUGC.StartItemUpdate(new AppId_t((uint)SteamManager.AppId), fileId);
        SteamUGC.SetItemTitle(currentUpdateHandle, nameField.text);
        SteamUGC.SetItemDescription(currentUpdateHandle, descField.text);
        SteamUGC.SetItemContent(currentUpdateHandle, folderPath); //create temporary folder somewhere to store the file, move the level file to that folder and rename it to the id of the newly created workshop item
        SteamUGC.SetItemPreview(currentUpdateHandle, previewPath);

        //we count how many launch and land pads are there to know if this is a coop level or not
        int[] padsCount = countPads();
        if (padsCount[0] == 2 && padsCount[1] == 2) {
            SteamUGC.SetItemTags(currentUpdateHandle, new string[] { "Co-op" }); //if there are two launch pads and two land pads, this is a coop level
        } else {
            SteamUGC.SetItemTags(currentUpdateHandle, new string[] { "Singleplayer" }); //otherwise this is a singleplayer level
        }

        SteamUGC.SetItemVisibility(currentUpdateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);

        if (needsToAcceptWorkshopLegalAgreement) {
            ShowWorkshopLegal();
            SteamUGC.DeleteItem(fileId);
            LevelEditorManager.GetLevelData().workshopId = 0;
            return;
        }

        steamWorkshopLogo.GetComponent<Button>().interactable = true;
        steamWorkshopLogo.sprite = viewOnWorkshopLogo;

        //save the level data into the level editor levels folder and copy it from there
        LevelEditorSaveMenu.SaveLevelForWorkshop(filePath, nameField.text, usingOverridePreview ? overridePreviewPath : "");
    }

    private void WorkshopItemCreated(CreateItemResult_t callback, bool result) {
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            PromptsController.OpenErrorPrompt("Error uploading workshop item! " + callback.m_eResult);
            return;
        }

        if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {

        }

        StartItemUpdate(callback.m_nPublishedFileId, callback.m_bUserNeedsToAcceptWorkshopLegalAgreement);
    }

    public void FinalPublishUpdateHandle() {
        if (originalPublish) {
            SteamCustomUtils.AddStat("WORKSHOP_LEVELS");
        }

        SteamAPICall_t apiCall = SteamUGC.SubmitItemUpdate(currentUpdateHandle, originalPublish ? "Original publish" : "Level update");
        SteamCallbacks.SubmitItemUpdateResult_t.RegisterCallResult(OnChangesSubmitted, apiCall);
    }

    private void OnChangesSubmitted(SubmitItemUpdateResult_t callback, bool error) {
        if (error || callback.m_eResult != EResult.k_EResultOK) {
            PromptsController.OpenErrorPrompt("Failed to update workshop item! Error code: " + callback.m_eResult.ToString());
            throw new Exception("CreateItemResult_t Failed - result = " + callback.m_eResult.ToString());
        }
        
        SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/filedetails/?id=" + callback.m_nPublishedFileId);

        overridePreviewPath = "";
        customPreviewImage.sprite = null;
        customPreviewImage.color = new Color(0, 0, 0, 0);

        //delete all files previously inside there
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.persistentDataPath + "/Level Editor/Temporary Workshop Upload/");
        foreach(FileInfo file in directoryInfo.GetFiles()) {
            file.Delete();
        }
    }

    public void ShowWorkshopLegal() {
        SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/workshoplegalagreement/?appid=" + SteamManager.AppId);
    }

    public void SetTitleAndDescription(string title, string description) {
        nameField.text = title;
        descField.text = description;
    }

    public void SetBackgroundPreviewSprite(Sprite previewSprite) {
        mainEscapeMenuBackground.color = new Color(1, 1, 1, 0.2f);
        mainEscapeMenuBackground.preserveAspect = true;
        mainEscapeMenuBackground.sprite = previewSprite;

        choosePreviewDefaultImage.preserveAspect = true;
        choosePreviewDefaultImage.sprite = previewSprite;
    }

    public void ShowChoosePreviewPrompt() {
        if(choosePreviewDefaultImage.sprite == null) {
            PromptsController.OpenErrorPrompt("You must first have a default preview image to select a custom one (you can get this by saving your level)");
            CloseChoosePreviewPrompt();
            return;
        }

        choosePreviewLerpGroup.target = 1;
    }

    public void CloseChoosePreviewPrompt() {
        choosePreviewLerpGroup.target = 0;
    }

    public void ChooseDefaultPreview() {
        overridePreviewPath = "";
        CloseChoosePreviewPrompt();
    }

    public void OpenCustomPreviewFileDialog() {
        /*var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
            new ExtensionFilter("All Files", "*" ),
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Select Level Preview Image", "", extensions, false);

        if(path.Length > 0) {
            FileInfo newPreviewInfo = new FileInfo(path[0]);

            if(newPreviewInfo.Exists && newPreviewInfo.Length < 1024 * 1024) { //if the file really exists and is less than a megabyte
                string[] imageExtensions = new string[] { ".jpg", ".jpeg", ".gif", ".png" };
                bool isFileImage = imageExtensions.Contains(newPreviewInfo.Extension);

                if (isFileImage) { //if the file is really an image
                    overridePreviewPath = path[0];
                    CloseChoosePreviewPrompt();

                    Texture2D previewTexture = new Texture2D(2, 2);
                    byte[] dataBytes = File.ReadAllBytes(overridePreviewPath);
                    previewTexture.LoadImage(dataBytes);
                    Sprite previewSprite = Sprite.Create(previewTexture, new Rect(0, 0, previewTexture.width, previewTexture.height), new Vector2(0.5f, 0.5f));
                    customPreviewImage.color = new Color(1, 1, 1, 1);
                    customPreviewImage.sprite = previewSprite;
                    customPreviewImage.preserveAspect = true;
                }
            }
        }*/

        var dialog = new Process();
        dialog.StartInfo.FileName = Environment.CurrentDirectory + @"\CustomDialog.exe";
        dialog.StartInfo.UseShellExecute = false;
        dialog.Start();
    }

    public void OpenLevelThemePrompt() {
        chooseLevelThemePrompt.target = 1;
    }

    public void CloseLevelThemePrompt() {
        chooseLevelThemePrompt.target = 0;
    }

    public void TemporarySetCustomPreviewIconPath(string path) {
        overridePreviewPath = path;

        CloseChoosePreviewPrompt();
    }

    public void TemporaryOpenCustomPreviewHelp() {
        SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/games/913600/announcements/detail/1684802565954844224");
    }
}
