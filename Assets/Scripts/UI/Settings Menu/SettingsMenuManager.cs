using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class SettingsMenuManager : MonoBehaviour {
    private static SettingsMenuManager Singletron;

    [Header("Tabs and panels")]
    public Image[] tabs;
    public CanvasGroup[] panels;

    [Header("Graphics")]
    public TMP_Dropdown resolutionsDropdown;
    public TMP_Dropdown screenModeDropdown;
    public TMP_Dropdown qualityDropdown;

    [Header("Audio")]
    public Slider musicVolumeSlider;
    public Slider effectsVolumeSlider;
    public Slider uiVolumeSlider;
    public AudioMixer audioMixer;

    [Header("Other")]
    public Toggle enableGhostReplay;
    public Toggle levelTimerToggle;

    private bool isOpen = false;
    private bool acceptInput = false;
    private Resolution[] resolutions;

    private Canvas selfCanvas;
    private GraphicRaycaster selfRaycaster;

    private void Awake() {
        if(Singletron != null) {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Singletron = this;

        selfCanvas = GetComponent<Canvas>();
        selfRaycaster = GetComponent<GraphicRaycaster>();

        CloseMenu();
        SelectTab(0);

        //assigning all resolution options
        resolutions = Screen.resolutions;

        List<string> newOptions = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++) {
            Resolution resolution = resolutions[i];
            newOptions.Add(resolution.ToString());
            if (resolution.width == Screen.width && resolution.height == Screen.height) currentResolutionIndex = i;
        }

        resolutionsDropdown.ClearOptions();
        resolutionsDropdown.AddOptions(newOptions);

        qualityDropdown.value = QualitySettings.GetQualityLevel();
        screenModeDropdown.value = (int)Screen.fullScreenMode;
        resolutionsDropdown.value = currentResolutionIndex;

        musicVolumeSlider.value = PlayerPrefs.GetFloat("musicVol", 1);
        effectsVolumeSlider.value = PlayerPrefs.GetFloat("effectsVol", 1);
        uiVolumeSlider.value = PlayerPrefs.GetFloat("uiVol", 1);
        
        levelTimerToggle.isOn = PlayerPrefs.GetInt("showLevelTimer", 1) == 1;
        enableGhostReplay.isOn = GhostReplay.Enabled;
    }

    private void Start() {
        UpdateVolumes();

        acceptInput = true;
    }

    private void Update() {
        if(isOpen && Input.GetKeyDown(KeyCode.Escape) && LastPressedEscape.LastPressedEscapeCooldownOver(0.2f)) {
            LastPressedEscape.SetPressedEscape();
            CloseMenu();
        }
    }

    public static void OpenMenu() {
        if (Singletron == null) return;

        Singletron._OpenMenu();
    }

    private void _OpenMenu() {
        SelectTab(0);

        CursorController.AddUser("settingsMenu");

        isOpen = true;
        selfCanvas.enabled = isOpen;
        selfRaycaster.enabled = isOpen;
    }

    public void CloseMenu() {
        isOpen = false;

        CursorController.RemoveUser("settingsMenu");

        selfCanvas.enabled = isOpen;
        selfRaycaster.enabled = isOpen;
    }

    public void SelectTab(int index) {
        for(int i = 0; i < tabs.Length; i++) {
            Image tab = tabs[i];

            if(i == index) {
                tab.color = Color.white;
            } else {
                tab.color = new Color(0.9481132f, 0.951237f, 1, 0.6784314f);
            }

            CanvasGroup panel = panels[i];
            panel.alpha = i == index ? 1 : 0;
            panel.blocksRaycasts = i == index;
        }
    }

    public void SetResolution(int index) {
        if (!acceptInput) return;

        Screen.SetResolution(resolutions[index].width, resolutions[index].height, Screen.fullScreenMode);
        Canvas.ForceUpdateCanvases();
    }

    public void SetQuality(int index) {
        if (!acceptInput) return;

        QualitySettings.SetQualityLevel(index);
    }

    public void SetFullscreen(int index) {
        if (!acceptInput) return;
        
        Screen.fullScreenMode = (FullScreenMode)index;
    }

    private void UpdateVolumes() {
        audioMixer.SetFloat("musicVol", Mathf.Lerp(-70, -28, PlayerPrefs.GetFloat("musicVol", 1)));
        audioMixer.SetFloat("effectsVol", Mathf.Lerp(-70, 0, PlayerPrefs.GetFloat("effectsVol", 1)));
        audioMixer.SetFloat("uiVol", Mathf.Lerp(-70, -30, PlayerPrefs.GetFloat("uiVol", 1)));
    }

    public void SetMusicVolume(float value) {
        if (!acceptInput) return;

        print(value);

        PlayerPrefs.SetFloat("musicVol", value);

        UpdateVolumes();
    }

    public void SetEffectsVolume(float value) {
        if (!acceptInput) return;

        PlayerPrefs.SetFloat("effectsVol", value);

        UpdateVolumes();
    }

    public void SetUIVolume(float value) {
        if (!acceptInput) return;

        PlayerPrefs.SetFloat("uiVol", value);

        UpdateVolumes();
    }

    public void SetEnableGhostReplay(bool on) {
        if (!acceptInput) return;

        GhostReplay.Enabled = on;
    }

    public void SetShowLevelTimer(bool on) {
        if (!acceptInput) return;

        PlayerPrefs.SetInt("showLevelTimer", on ? 1 : 0);
    }

    public void SetInvertedControls(bool inverted) {
        if (!acceptInput) return;

        PlayerPrefs.SetInt("invertControls", inverted ? 1 : 0);
    }

    public void ResetLevelProgress() {
        LevelProgressCounter.LevelProgressList = new List<LevelProgress>();
        LevelProgressCounter.UpdateFile();

        SteamCustomUtils.SetStat("SP_PLAYED", 0);
        SteamCustomUtils.SetStat("COOP_PLAYED", 0);

        if (MainMenuManager.Singletron != null) MainMenuManager.Singletron.UpdateAverageScoreText();

        GameEvent currentEvent = GameEvents.GetCurrentEvent();
        if (currentEvent != null) {
            GameEvents.CurrentEventProgression = 0;
            GameEvents.CurrentEventCoopProgression = 0;
        }
    }
}
