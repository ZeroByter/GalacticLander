using System.Collections;
using System.IO;
using UnityEngine;
using TMPro;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using System.Collections.Generic;
using Steamworks;
using UnityEditor;

public class LevelEditorSaveMenu : MonoBehaviour {
    private static LevelEditorSaveMenu Singletron;

    [Header("The transition")]
    public CanvasBlurTransition transition;
    [Header("Level template")]
    public SavedLevelItemController template;
    [Header("Save level panel")]
    public GameObject saveLevelPrompt;
    public TMP_InputField newSaveLevelNameInput;
    [Header("Migrate old level panel")]
    public GameObject migrateOldLevelPrompt;
    public Button beginCurrentLevelMigrationButton;
    [Header("Other UI stuff")]
    public GameObject levelInfoPanel;
    public Image levelPreviewImage;
    public TMP_Text levelName;
    public TMP_InputField levelNameInput;
    public TMP_Text levelStats;
    public Button loadCurrentLevelButton;
    [Header("Camera transition stuff")]
    public GameObject mainCamera;
    public GameObject screenshotCanvas;
    public TMP_Text screenshotTextWhite;
    public Camera screenshotCamera;

    /// <summary>
    /// Are we currently saving the level for later upload to the steam workshop?
    /// </summary>
    private bool savingForWorkshop = false;
    /// <summary>
    /// Overrides the level name text that appears when taking a screenshot
    /// </summary>
    private string screenshotTextOverride;
    /// <summary>
    /// If the user is using his own custom preview file, we copy that instead
    /// </summary>
    private string copyCustomPreviewPath;
    /// <summary>
    /// The path we will want to move the workshop level into once we save it
    /// </summary>
    private string workshopNewPath = "";

    private LevelData currentLevelData;
    private string currentLevelFileDirectory;

    private string selectedLevelName;
    private string saveNewLevelName;

    private void Awake() {
        Singletron = this;

        template.gameObject.SetActive(false);

        loadCurrentLevelButton.onClick.AddListener(HandleLoadCurrentLevelButtonClick);
        beginCurrentLevelMigrationButton.onClick.AddListener(HandleBeginCurrentLevelMigrationClick);
    }

    private void OnDestroy()
    {
        loadCurrentLevelButton.onClick.RemoveListener(HandleLoadCurrentLevelButtonClick);
        beginCurrentLevelMigrationButton.onClick.RemoveListener(HandleBeginCurrentLevelMigrationClick);
    }

    /// <summary>
    /// called to mark that the next time we save a level we are NOT doing it in order to upload to the workshop
    /// </summary>
    public void DisableSavingForWorkshop() {
        savingForWorkshop = false;
        workshopNewPath = "";
        screenshotTextOverride = "";
    }

    private void Update() {
        if (transition.isOpen && Input.GetKeyDown(KeyCode.Escape) && LastPressedEscape.LastPressedEscapeCooldownOver(0.1f)) {
            transition.CloseMenu();
            LastPressedEscape.SetPressedEscape();
        }

        if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
            if (Input.GetKeyDown(KeyCode.O)){
                selectedLevelName = "";
                ShowOpenLevel();
            }

            if (Input.GetKeyDown(KeyCode.S)) {
                DisableSavingForWorkshop();

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    selectedLevelName = "";
                    saveNewLevelName = "";
                }

                if (string.IsNullOrEmpty(saveNewLevelName)) {
                    ShowSaveLevel();
                } else {
                    SaveNewLevel();
                }
            }
        }
    }

    private void CreateTemplate(string dir) {
        if (File.Exists(dir)) {
            SavedLevelItemController newTemplate = Instantiate(template, template.transform.parent);
            newTemplate.Setup(dir);
        }
    }

    private void LoadLevelsList() {
        currentLevelData = null;
        currentLevelFileDirectory = "";
        levelInfoPanel.SetActive(false);
        foreach (Transform oldTemplate in template.transform.parent) {
            if (oldTemplate.gameObject.activeSelf) Destroy(oldTemplate.gameObject);
        }

        string levelsDir = Application.persistentDataPath + "/Level Editor/Levels/";

        Directory.CreateDirectory(levelsDir);

        //go through all files in this
        foreach(string fileDir in Directory.GetFiles(levelsDir)) {
            if (fileDir.EndsWith(".level")){
                CreateTemplate(fileDir);
            }
        }
    }

    public void ShowOpenLevel() {
        transition.OpenMenu();

        saveLevelPrompt.SetActive(false);
        migrateOldLevelPrompt.SetActive(false);

        LoadLevelsList();
    }

    public void ShowSaveLevel()
    {
        transition.OpenMenu();

        migrateOldLevelPrompt.SetActive(false);
        saveLevelPrompt.SetActive(true);
        newSaveLevelNameInput.text = "";
        newSaveLevelNameInput.ActivateInputField();

        LoadLevelsList();

        LevelEditorEscapeMenuController.Singletron.CheckIfShouldBeOpen();
    }

    public void ShowMigrateOldLevel()
    {
        transition.OpenMenu();

        migrateOldLevelPrompt.SetActive(true);
        saveLevelPrompt.SetActive(false);

        LevelEditorEscapeMenuController.Singletron.CheckIfShouldBeOpen();
    }

    private Sprite TextureToSprite(Texture2D texture) {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public void SelectLevel(LevelData levelData, string fileName) {
        currentLevelData = levelData;
        currentLevelFileDirectory = fileName;
        ShowLevelName();
        levelInfoPanel.SetActive(true);

        levelName.text = Path.GetFileName(currentLevelFileDirectory).Replace(".level", "");
        selectedLevelName = levelName.text;

        StringBuilder levelStatsString = new StringBuilder();
        levelStatsString.AppendLine(string.Format("Time created: {0} {1}", levelData.timeCreated.ToLongTimeString(), levelData.timeCreated.ToLongDateString()));
        levelStatsString.AppendLine();
        levelStatsString.AppendLine(string.Format("Time modified: {0} {1}", levelData.lastModified.ToLongTimeString(), levelData.lastModified.ToLongDateString()));
        levelStats.text = levelStatsString.ToString();

        string previewImageDirectory = currentLevelFileDirectory.Replace(".level", ".png");
        if (File.Exists(previewImageDirectory)) {
            levelPreviewImage.transform.parent.gameObject.SetActive(true);

            Texture2D previewTexture = new Texture2D(2, 2);
            byte[] dataBytes = File.ReadAllBytes(previewImageDirectory); ;
            previewTexture.LoadImage(dataBytes);
            levelPreviewImage.sprite = TextureToSprite(previewTexture);
            levelPreviewImage.preserveAspect = true;
        } else {
            levelPreviewImage.transform.parent.gameObject.SetActive(false);
        }
    }

    public void ShowLevelName() {
        levelName.gameObject.SetActive(true);
        levelNameInput.gameObject.SetActive(false);
    }

    public void ShowLevelNameInput() {
        levelName.gameObject.SetActive(false);
        levelNameInput.gameObject.SetActive(true);
        levelNameInput.text = Path.GetFileName(currentLevelFileDirectory).Replace(".level", "");
        levelNameInput.ActivateInputField();
    }

    public void ChangeCurrentLevelName(string newName) {
        ShowLevelName();
        if (!Input.GetKeyDown(KeyCode.Escape)) {
            string newFileDirectory = currentLevelFileDirectory.Replace(Path.GetFileName(currentLevelFileDirectory), newName + ".level");
            File.Move(currentLevelFileDirectory, newFileDirectory);
            if (File.Exists(currentLevelFileDirectory.Replace(".level", ".png"))) File.Move(currentLevelFileDirectory.Replace(".level", ".png"), newFileDirectory.Replace(".level", ".png"));

            currentLevelFileDirectory = newFileDirectory;
            levelName.text = newName;
            LoadLevelsList();
        } else {
            LastPressedEscape.SetPressedEscape();
        }
    }

    #region Screenshot camera positioning
    private List<Vector3> screenshotCameraPositions = new List<Vector3>();

    private Vector3 GetAveragePosition() {
        float x = 0;
        float y = 0;

        foreach (Vector3 position in screenshotCameraPositions) {
            x += position.x;
            y += position.y;
        }

        return new Vector3(x / screenshotCameraPositions.Count, y / screenshotCameraPositions.Count, -50);
    }

    private float GetRequiredSize() {
        Vector3 desiredLocalPos = transform.InverseTransformPoint(GetAveragePosition());

        float size = 0;

        foreach (Vector3 position in screenshotCameraPositions) {
            Vector3 targetLocalPos = transform.InverseTransformPoint(position);
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / screenshotCamera.aspect);
        }

        size += 5;
        //size = Mathf.Max(size, 5);

        return size;
    }

    private void PositionScreenshotCamera() {
        if (LevelEditorManager.GetLevelData().levelData.Count <= 2) return;

        screenshotCameraPositions = new List<Vector3>();
        screenshotCameraPositions.Add(LevelEditorManager.GetLevelData().GetBounds().min);
        screenshotCameraPositions.Add(LevelEditorManager.GetLevelData().GetBounds().max);
        /*foreach(LevelObject obj in LevelEditorManager.GetLevelData().levelData) {
            screenshotCameraPositions.Add(obj.GetPosition());
        }*/

        screenshotCamera.transform.position = GetAveragePosition();
        screenshotCamera.orthographicSize = GetRequiredSize();
    }
    #endregion

    public IEnumerator TakeScreenshot(string overridePath = "") {
        if (string.IsNullOrEmpty(screenshotTextOverride)) {
            screenshotTextWhite.text = saveNewLevelName;
        } else {
            screenshotTextWhite.text = screenshotTextOverride;
        }
        PositionScreenshotCamera();
        
        yield return new WaitForEndOfFrame();

        string levelsDir = Application.persistentDataPath + "/Level Editor/Levels/";

        int width = Screen.width;
        int height = Screen.height;
        
        RenderTexture rt = new RenderTexture(width, height, 24);
        screenshotCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshotCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenShot.EncodeToPNG();

        if(string.IsNullOrEmpty(overridePath)) {
            File.WriteAllBytes(levelsDir + saveNewLevelName + ".png", bytes);
        } else {
            File.WriteAllBytes(overridePath, bytes);
        }

        //we can apply texture2d to escape menu image
        Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshotTexture.LoadImage(bytes);

        LevelEditorEscapeMenuController.Singletron.SetBackgroundPreviewSprite(TextureToSprite(screenshotTexture));
        
        screenshotTextWhite.text = "";

        if (savingForWorkshop) {
            LevelEditorEscapeMenuController.Singletron.FinalPublishUpdateHandle();
        }
    }

    private void BeginTakeScreenshot(string saveToPath = "") {
        StartCoroutine(TakeScreenshot(saveToPath));
    }

    public static void StaticTakeScreenshot(string saveToPath = "") {
        if (Singletron == null) return;

        Singletron.BeginTakeScreenshot(saveToPath);
    }

    private void SaveNewLevel() {
        string levelsDir = Application.persistentDataPath + "/Level Editor/Levels/";
        FileStream file = File.Open(levelsDir + saveNewLevelName + ".level", FileMode.OpenOrCreate);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, LevelEditorManager.GetLevelData());
        file.Close();

        if (savingForWorkshop) {
            File.Copy(levelsDir + saveNewLevelName + ".level", workshopNewPath);

            if (string.IsNullOrEmpty(copyCustomPreviewPath)) { //if we are not using a custom preview image
                BeginTakeScreenshot(workshopNewPath.Replace(".level", ".png")); //take a screenshot and store it where the new level folder is going to get uploaded
            } else { //if we are using a custom preview image
                string customPreviewExtension = new FileInfo(copyCustomPreviewPath).Extension;

                File.Copy(copyCustomPreviewPath, workshopNewPath.Replace(".level", customPreviewExtension)); //move the custom preview image into the workshop upload folder
                BeginTakeScreenshot(); //take a normal screenshot
            }
        } else {
            BeginTakeScreenshot();
        }

        transition.CloseMenu();
    }

    public void SaveNewLevelInput(string newLevelName) {
        if(Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2)) return;

        if (!string.IsNullOrEmpty(newLevelName)) {
            saveNewLevelName = newLevelName;

            SaveNewLevel();
        }
    }

    public void SaveNewLevelButton() {
        if (!string.IsNullOrEmpty(newSaveLevelNameInput.text)) {
            saveNewLevelName = newSaveLevelNameInput.text;

            SaveNewLevel();
        }
    }

    public void DeleteCurrentLevel() {
        if (File.Exists(currentLevelFileDirectory)) {
            File.Delete(currentLevelFileDirectory);
            if(File.Exists(currentLevelFileDirectory.Replace(".level", ".png"))) File.Delete(currentLevelFileDirectory.Replace(".level", ".png"));

            LoadLevelsList();
            currentLevelData = null;
            currentLevelFileDirectory = "";
            levelInfoPanel.SetActive(false);
        }
    }

    public void LoadCurrentLevelToEditor(string levelFileName, bool skipShowMigratePrompt = false) {
        if(currentLevelData != null) {
            saveNewLevelName = selectedLevelName;

            if (!string.IsNullOrEmpty(levelFileName)) {
                saveNewLevelName = levelFileName;
            }

            if(currentLevelData.levelMapValues == null && !skipShowMigratePrompt)
            {
                ShowMigrateOldLevel();
                return;
            }

            transition.CloseMenu();
            LevelEditorManager.SetLevelData(currentLevelData);

            //load the sprite
            string previewImageDirectory = currentLevelFileDirectory.Replace(".level", ".png");
            if (File.Exists(previewImageDirectory)) {
                Texture2D previewTexture = new Texture2D(2, 2);
                byte[] dataBytes = File.ReadAllBytes(previewImageDirectory); ;
                previewTexture.LoadImage(dataBytes);
                LevelEditorEscapeMenuController.Singletron.SetBackgroundPreviewSprite(TextureToSprite(previewTexture));
            }

            if (currentLevelData.workshopId != 0) {
                UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[]{ new PublishedFileId_t(currentLevelData.workshopId) }, 1);
                SteamUGC.SetReturnMetadata(handle, false);
                SteamUGC.SetReturnAdditionalPreviews(handle, false);
                SteamUGC.SetReturnChildren(handle, false);
                SteamUGC.SetAllowCachedResponse(handle, 4);
                SteamAPICall_t apiCall = SteamUGC.SendQueryUGCRequest(handle);
                SteamCallbacks.SteamUGCQueryCompleted_t.RegisterCallResult(SteamUGCQueryCompleted, apiCall);
            } else {
                LevelEditorEscapeMenuController.Singletron.SetTitleAndDescription("", "");
            }
        }
    }

    private void HandleLoadCurrentLevelButtonClick()
    {
        LoadCurrentLevelToEditor("", false);
    }

    private void HandleBeginCurrentLevelMigrationClick()
    {
        LoadCurrentLevelToEditor("", true);
    }

    private void SteamUGCQueryCompleted(SteamUGCQueryCompleted_t callback, bool error) {
        if (callback.m_eResult != EResult.k_EResultOK) {
            SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
            throw new System.Exception("Got UGC query result with " + callback.m_eResult.ToString());
        }

        for(int i = 0; i < callback.m_unNumResultsReturned; i++) {
            SteamUGCDetails_t details;
            if(SteamUGC.GetQueryUGCResult(callback.m_handle, (uint) i, out details)) {
                if(details.m_eResult == EResult.k_EResultFileNotFound) {
                    currentLevelData.workshopId = 0;
                    LevelEditorEscapeMenuController.Singletron.SetTitleAndDescription("", "");
                } else {
                    LevelEditorEscapeMenuController.Singletron.SetTitleAndDescription(details.m_rgchTitle, details.m_rgchDescription);
                }
            }
        }

        SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
    }

    /// <summary>
    /// saves the level and copies it into the workshop upload folder for future upload
    /// </summary>
    public static void SaveLevelForWorkshop(string newPath, string screenshotTextOverride, string copyCustomPreviewPath) {
        Singletron.savingForWorkshop = true;
        Singletron.workshopNewPath = newPath;
        Singletron.screenshotTextOverride = screenshotTextOverride;
        Singletron.copyCustomPreviewPath = copyCustomPreviewPath;

        if (string.IsNullOrEmpty(Singletron.saveNewLevelName)) {
            Singletron.ShowSaveLevel();
        } else {
            Singletron.SaveNewLevel();
        }
    }

    public void ShowLevelsFolderExplorer() {
        string levelsDir = @Application.@persistentDataPath + @"/Level Editor/Levels/";
        /*levelsDir = levelsDir.Replace(@"/", @"\");
        System.Diagnostics.Process.Start("explorer.exe", @levelsDir);*/
        OpenInWinFileBrowser(levelsDir);
    }

    private static void OpenInWinFileBrowser(string path) {
        // try windows
        string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

        try {
            System.Diagnostics.Process.Start(winPath);
        } catch (System.ComponentModel.Win32Exception e) {
            // tried to open win explorer in mac
            // just silently skip error
            // we currently have no platform define for the current OS we are in, so we resort to this
            e.HelpLink = ""; // do anything with this variable to silence warning about not using it
        }
    }
}
