using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SavedLevelItemController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static Dictionary<string, SavedLevelItemController> Controllers = new Dictionary<string, SavedLevelItemController>();

    public LevelEditorSaveMenu saveMenu;
    [Header("TMP Text elements")]
    public TMP_Text levelName;
    public TMP_Text levelLastModified;

    private string levelFileDirectory;
    private LevelData levelData;

    private bool isHovering;
    private string errorString;
    private float lastClicked;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    void Update()
    {
        if (isHovering && !string.IsNullOrEmpty(errorString)) TooltipController.ActivateTooltip(errorString);
    }

    private void LoadLevelData()
    {
        FileStream file = File.Open(levelFileDirectory, FileMode.Open);

        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            levelName.text = Path.GetFileName(levelFileDirectory).Replace(".level", "");
            levelData = (LevelData)bf.Deserialize(file);
            if (levelData != null)
            {
                levelLastModified.text = levelData.lastModified.ToShortTimeString() + " " + levelData.lastModified.ToShortDateString();
            }
        }
        catch (SerializationException)
        {
            levelName.color = Color.red;
            levelLastModified.color = Color.red;
            errorString = "Serialization error - file data is corrupt!";
        }

        file.Close();
    }

    public void Setup(string levelFileDir, bool isSelected)
    {
        levelFileDirectory = levelFileDir;
        LoadLevelData();

        Controllers[levelName.text] = this;

        gameObject.SetActive(true);

        if (isSelected)
        {
            TriggerSelectLevel();
        }
    }

    public void TriggerSelectLevel()
    {
        saveMenu.SelectLevel(levelData, levelFileDirectory);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (levelData != null)
        {
            if (Time.time - lastClicked < 0.2f)
            {
                if (saveMenu.isCurrentlySaving)
                {
                    saveMenu.SaveNewLevel();
                }
                else
                {
                    saveMenu.LoadCurrentLevelToEditor(levelName.text, false);
                }
            }
            else
            {
                if (saveMenu.isCurrentlySaving)
                {
                    LevelEditorSaveMenu.SetSaveLevelName(levelName.text);
                }
                else
                {
                    TriggerSelectLevel();
                }
            }

            lastClicked = Time.time;
        }
    }
}
