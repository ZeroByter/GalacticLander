using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorFileDialogController : MonoBehaviour
{
    public Transform folderTemplate;
    public Transform fileTemplate;
    public Button confirmButton;
    public TMP_Text currentDirectoryText;
    [Header("The folder icon which will be placed on folder 'file' templates")]
    public Sprite folderIconSprite;
    [Header("The escape menu controller")]
    public LevelEditorEscapeMenuController escapeMenuController;

    private LerpCanvasGroup lerpCanvasGroup;

    private bool isVisisble;
    //TODO: use for filewatcher
    private string currentDirectory;
    private string selectedFile;

    private List<LerpCanvasGroup> fileBackgrounds = new List<LerpCanvasGroup>();

    private void Awake()
    {
        lerpCanvasGroup = GetComponent<LerpCanvasGroup>();

        lerpCanvasGroup.ForceAlpha(0);
        Hide();

        fileTemplate.gameObject.SetActive(false);
        folderTemplate.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isVisisble && Input.GetKeyDown(KeyCode.Escape) && LastPressedEscape.LastPressedEscapeCooldownOver(0.1f))
        {
            LastPressedEscape.SetPressedEscape();

            Hide();
        }
    }

    public void Show()
    {
        isVisisble = true;

        lerpCanvasGroup.target = 1;

        OpenDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    }

    public void Hide()
    {
        isVisisble = false;

        lerpCanvasGroup.target = 0;
    }

    public void OpenExplorer()
    {
        Process.Start(Application.persistentDataPath + @"/Level Editor/Levels");
    }

    public void SelectFile(Transform file, string filePath, int index)
    {
        for(int i = 0; i < fileBackgrounds.Count; i++)
        {
            LerpCanvasGroup group = fileBackgrounds[i];

            //Debug.Log($"{group}, i = {i} - index = {index}", group);

            group.target = i == index ? 0.15f : 0f;
        }

        selectedFile = filePath;

        confirmButton.interactable = true;
    }

    public void ConfirmSelection()
    {
        escapeMenuController.TemporarySetCustomPreviewIconPath(selectedFile);

        Hide();
    }

    private void ClearTemplateParent(Transform clearParentOf)
    {
        foreach (Transform oldTemplate in clearParentOf.parent)
        {
            if (oldTemplate.gameObject.activeSelf)
            {
                Destroy(oldTemplate.gameObject);
            }
        }
    }

    private void ClearDirectories()
    {
        ClearTemplateParent(folderTemplate);
    }

    private void ClearFiles()
    {
        ClearTemplateParent(fileTemplate);
    }

    private bool IsImageFile(FileInfo file)
    {
        if (file.Name.EndsWith(".png")) return true;
        if (file.Name.EndsWith(".jpg")) return true;
        if (file.Name.EndsWith(".jpeg")) return true;
        if (file.Name.EndsWith(".bmp")) return true;
        return false;
    }

    private Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void CreateFileTemplate(FileInfo file)
    {
        Transform newTemplate = Instantiate(fileTemplate, fileTemplate.parent);
        newTemplate.GetComponentInChildren<TMP_Text>().text = file.Name;
        Button button = newTemplate.GetComponent<Button>();

        if (IsImageFile(file))
        {
            Image icon = newTemplate.GetChild(1).GetComponent<Image>();

            Texture2D previewTexture = new Texture2D(2, 2);
            byte[] dataBytes = File.ReadAllBytes(file.FullName); ;
            previewTexture.LoadImage(dataBytes);
            icon.sprite = TextureToSprite(previewTexture);
            icon.preserveAspect = true;

            var colors = button.colors;
            colors.normalColor = new Color(1,1,1,0.75f);
            colors.highlightedColor = new Color(1, 1, 1, 0.85f);
            colors.pressedColor = new Color(1, 1, 1, 1);
            button.colors = colors;
        }

        button.interactable = IsImageFile(file);

        var selectedBackgroundGroup = newTemplate.GetChild(0).GetComponent<LerpCanvasGroup>();
        selectedBackgroundGroup.ForceAlpha(0);
        fileBackgrounds.Add(selectedBackgroundGroup);

        button.onClick.AddListener(delegate { SelectFile(newTemplate, file.FullName, fileBackgrounds.Count - 1); });

        newTemplate.gameObject.SetActive(true);
    }

    private void CreateFileTemplate(DirectoryInfo folder, string customName = "")
    {
        Transform newTemplate = Instantiate(fileTemplate, fileTemplate.parent);
        if (!string.IsNullOrEmpty(customName))
        {
            newTemplate.GetComponentInChildren<TMP_Text>().text = $"/{customName}";
        }
        else
        {
            newTemplate.GetComponentInChildren<TMP_Text>().text = $"/{folder.Name}";
        }
        Button button = newTemplate.GetComponent<Button>();

        Image icon = newTemplate.GetChild(1).GetComponent<Image>();
        icon.sprite = folderIconSprite;

        button.interactable = true;
        button.onClick.AddListener(delegate { OpenDirectory(folder.FullName); });

        newTemplate.gameObject.SetActive(true);
    }

    private void CreateFolderTemplate(DirectoryInfo folder, string customName = "", bool includeIcon = false)
    {
        Transform newTemplate = Instantiate(folderTemplate, folderTemplate.parent);
        var text = newTemplate.GetComponentInChildren<TMP_Text>();
        if (!string.IsNullOrEmpty(customName))
        {
            text.text = $"/{customName}";
        }
        else
        {
            text.text = $"/{folder.Name}";
        }

        if (includeIcon)
        {
            text.text = $"<sprite=0> {text.text}";
        }

        Button button = newTemplate.GetComponent<Button>();
        button.interactable = true;
        button.onClick.AddListener(delegate { OpenDirectory(folder.FullName); });

        newTemplate.gameObject.SetActive(true);
    }

    private Color ParseHtmlString(string hex)
    {
        Color color = Color.white;

        if(ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }

        return color;
    }

    private void CreateFolderDivider(string textString)
    {
        var newTemplate = Instantiate(folderTemplate, folderTemplate.parent);
        Destroy(newTemplate.GetComponent<CanvasRenderer>());
        Destroy(newTemplate.GetComponent<Button>());
        Destroy(newTemplate.GetComponent<PointerOnHover>());
        Destroy(newTemplate.GetComponent<ButtonHoverSound>());

        var text = newTemplate.GetComponentInChildren<TMP_Text>();
        text.text = textString;
        text.color = ParseHtmlString("#323232");

        newTemplate.gameObject.SetActive(true);

    }

    //TODO: Don't forget filewatcher!

    private void OpenDirectory(string directory)
    {
        currentDirectoryText.text = directory;

        selectedFile = "";

        confirmButton.interactable = false;
        
        currentDirectory = directory;

        ClearDirectories();
        ClearFiles();

        var directoryInfo = new DirectoryInfo(directory);
        
        CreateFolderTemplate(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)), "<sprite=0> Desktop");

        CreateFolderDivider("Drives:");
        foreach(var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady) continue;
            CreateFolderTemplate(new DirectoryInfo(drive.Name), $"{drive.Name.Replace("/", "")}");
        }

        CreateFolderDivider("Folders:");
        if (directoryInfo.Parent != null)
        {
            CreateFileTemplate(directoryInfo.Parent, "..");
            CreateFolderTemplate(directoryInfo.Parent, "..", true);
        }

        foreach (var folder in directoryInfo.GetDirectories())
        {
            CreateFileTemplate(folder);
            CreateFolderTemplate(folder, "", true);
        }

        foreach (var file in directoryInfo.GetFiles())
        {
            if (IsImageFile(file)) CreateFileTemplate(file);
        }
    }
}
