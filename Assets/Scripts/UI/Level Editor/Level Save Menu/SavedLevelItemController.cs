using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class SavedLevelItemController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    public LevelEditorSaveMenu saveMenu;
    [Header("TMP Text elements")]
    public TMP_Text levelName;
    public TMP_Text levelLastModified;

    private string levelFileDirectory;
    private LevelData levelData;

    private bool isHovering;
    private string errorString;
    private float lastClicked;

    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
    }

    void Update() {
        if (isHovering && !string.IsNullOrEmpty(errorString)) TooltipController.ActivateTooltip(errorString);
    }

    private void LoadLevelData() {
        FileStream file = File.Open(levelFileDirectory, FileMode.Open);

        try {
            BinaryFormatter bf = new BinaryFormatter();
            levelName.text = Path.GetFileName(levelFileDirectory).Replace(".level", "");
            levelData = (LevelData) bf.Deserialize(file);
            if(levelData != null) {
                levelLastModified.text = levelData.lastModified.ToShortTimeString() + " " + levelData.lastModified.ToShortDateString();
            }
        } catch (SerializationException) {
            levelName.color = Color.red;
            levelLastModified.color = Color.red;
            errorString = "Serialization error - file data is corrupt!";
        }

        file.Close();
    }

    public void Setup(string levelFileDir) {
        levelFileDirectory = levelFileDir;
        LoadLevelData();

        gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(Time.time - lastClicked < 0.2f) {
            if (levelData != null) {
                saveMenu.LoadCurrentLevelToEditor(levelName.text, false);
            }
        } else {
            if (levelData != null) {
                saveMenu.SelectLevel(levelData, levelFileDirectory);
            }
        }

        lastClicked = Time.time;
    }
}
