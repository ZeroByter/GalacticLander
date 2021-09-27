using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SourceConsole;

public class LevelEditorTutorialManager : MonoBehaviour {
    private static LevelEditorTutorialManager Singleton;

    public static void StartTutorial()
    {
        if (Singleton == null) return;

        Singleton.StartCoroutine(Singleton.EnableTutorial());
    }

    [Header("UI stuff")]
    [SerializeField]
    private LerpCanvasGroup tutorialTextCanvasGroup;
    [SerializeField]
    private TMP_Text tutorialText;
    [SerializeField]
    private Button nextStageButton;
    [Header("Actual tutorial elements")]
    [SerializeField]
    private GameObject buildLevelOutline;
    [SerializeField]
    private GameObject launchHalo;
    [SerializeField]
    private GameObject theRealLandPad;
    [SerializeField]
    private GameObject landHalo;
    [SerializeField]
    private GameObject sensorHalo;
    [SerializeField]
    private GameObject doorHalo;

    [SerializeField]
    private RectTransform rectTransform;
    private Rect rect;

    private int lastTutorialStage = 0;
    private int tutorialStage = 0;
    private bool enableTutorial = false;

    [ConVar]
    public static int LevelEditorTutorial_Stage {
        get
        {
            if (Singleton == null) return -1;

            return Singleton.tutorialStage;
        }
        set
        {
            if (Singleton == null) return;

            Singleton.tutorialStage = value;
        }
    }

    private LevelEntity doorEntity;
    private LevelEntity sensorEntity;

    private bool shouldFollowWorktablePosition;
    private Vector2 followWorktablePosition;

    [ConVar]
    public static bool LevelEditorTutorial_ShouldFollowPosition
    {
        get
        {
            if (Singleton == null) return false;

            return Singleton.shouldFollowWorktablePosition;
        }
        set
        {
            if (Singleton == null) return;

            Singleton.shouldFollowWorktablePosition = value;
        }
    }

    [ConCommand]
    public static void LevelEditorTutorial_FollowPosition(float x, float y)
    {
        if (Singleton == null) return;
        Singleton.followWorktablePosition = new Vector2(x, y);
    }

    private LevelData levelData;

    private void Awake()
    {
        Singleton = this;

        tutorialTextCanvasGroup.ForceAlpha(0);

        buildLevelOutline.SetActive(false);
        launchHalo.SetActive(false);
        landHalo.SetActive(false);
        sensorHalo.SetActive(false);
        doorHalo.SetActive(false);
        
        nextStageButton.interactable = false;

        rect = rectTransform.rect;
    }

    private void Start()
    {
        levelData = LevelEditorManager.GetLevelData();
    }

    private IEnumerator EnableTutorial()
    {
        yield return new WaitForSeconds(0.45f);

        UpdateTutorialText();
        enableTutorial = true;

        StartCoroutine(CheckNextStageConditionsLoop());
    }

    private IEnumerator CheckNextStageConditionsLoop()
    {
        while (true)
        {
            CheckNextStageConditions();

            if (SceneManager.GetActiveScene().name != "Level Editor") break;

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnDestroy()
    {
        Singleton = null;
    }

    public static void TriedToDeletePads()
    {
        if (Singleton == null) return;

        if(Singleton.tutorialStage == 2)
        {
            ShowTutorialText("Well, anything you want to delete EXCEPT the land or launch pads... We still need those! (Click to continue)");
            SteamCustomUtils.SetLevelEditorAchievement("DONT_DELETE_THAT");
        }
    }

    public static void ShowTutorialText(string text)
    {
        if (Singleton == null) return;

        Singleton.tutorialTextCanvasGroup.target = 1;
        Singleton.tutorialText.text = text;
    }

    public static void HideTutorialText()
    {
        if (Singleton == null) return;

        Singleton.tutorialTextCanvasGroup.target = 0;
    }

    private void UpdateTutorialText()
    {
        switch (tutorialStage)
        {
            case 0:
                ShowTutorialText("Hello! Let's begin by building a simple level outline. Hold down the left mouse button and drag the mouse over the white arrow outline. It doesn't have to be exact, but make sure that every tile has atleast one non-diagonal tile neighbor (meaning no diagonal corners)");
                buildLevelOutline.SetActive(true);
                break;
            case 1:
                ShowTutorialText("Excellent! Now let's move the landing pad to it's correct place, highlighted by the red fading icon to the right.");
                buildLevelOutline.SetActive(false);
                landHalo.SetActive(true);

                shouldFollowWorktablePosition = true;
                break;
            case 2:
                shouldFollowWorktablePosition = false;
                landHalo.SetActive(false);
                nextStageButton.interactable = true;
                CursorController.RemoveUser("HoverPointer");
                ShowTutorialText("Mistakes happen, and sometimes we place down tiles or entities we don't want. Luckily we have the eraser tool! Press 'A' on your keyboard to toggle between the eraser tool and hold down the left mouse button over anything you want to erase. (Click to continue)");
                break;
            case 3:
                shouldFollowWorktablePosition = true;
                followWorktablePosition = doorHalo.transform.position + new Vector3(0, -3);
                doorHalo.SetActive(true);
                ShowTutorialText("Lets place down a door in the middle of the level, in-between the launch and landing pad. Click the icon-button titled 'door' from the left panel and place the door over the highlighted area.");
                break;
            case 4:
                doorHalo.SetActive(false);
                followWorktablePosition = sensorHalo.transform.position + new Vector3(0, -1);
                ShowTutorialText("Great! Now we need a way to open the door. Place down a 'ship sensor pad' at the highlighted location (or anywhere else thats <i>inside</i> the level, keep in mind the player will need a way to get to the sensor).");
                sensorHalo.SetActive(true);
                break;
            case 5:
                sensorHalo.SetActive(false);
                followWorktablePosition = Vector2.Lerp(sensorHalo.transform.position, doorHalo.transform.position, 0.5f);
                followWorktablePosition.y = -1;
                ShowTutorialText("Now, if only the door and ship sensor were connected somehow! Hold down the left-alt key and click on the ship sensor, then on the door, and let go of the alt key to logic-connect the door and the ship sensor.");
                break;
            case 6:
                nextStageButton.interactable = true;
                shouldFollowWorktablePosition = false;
                ShowTutorialText("All done! If the level is enough to your liking, save your level by pressing 'CTRL+S', press 'escape' and click the 'playtest' button at the top of the screen!");
                break;
            case 14:
                ShowTutorialText("That's it... There is nothing else here...");
                break;
            default:
                nextStageButton.interactable = true;
                tutorialStage++;
                break;
        }
    }

    //TODO: Move this to regular level editor scene!

    private void GetIfPadsInsideLevel(out bool launchPadsInsideLevel, out bool landingPadsInsideLevel)
    {
        int numberOfLaunchingPads = 0;
        int numberOfLandingPads = 0;
        launchPadsInsideLevel = true;
        landingPadsInsideLevel = true;
        foreach (LevelObject obj in levelData.levelData)
        {
            if (obj.GetType() == typeof(LevelEntity))
            {
                LevelEntity ent = (LevelEntity)obj;
                if (ent.resourceName == "Ship Pads/Launch Pad")
                {
                    numberOfLaunchingPads++;
                    if (!levelData.IsPointInLevel(ent.GetPosition()))
                    {
                        launchPadsInsideLevel = false;
                    }
                }
                if (ent.resourceName == "Ship Pads/Land Pad")
                {
                    numberOfLandingPads++;
                    if (!levelData.IsPointInLevel(ent.GetPosition()))
                    {
                        landingPadsInsideLevel = false;
                    }
                }
            }
        }
    }

    private void CheckNextStageConditions()
    {
        levelData.SortTilesList();
        levelData.SortLines();

        switch (tutorialStage)
        {
            case 0:
                bool launchPadsInsideLevel;
                bool landingPadsInsideLevel;
                GetIfPadsInsideLevel(out launchPadsInsideLevel, out landingPadsInsideLevel);

                if (levelData.IsLevelInclosed() && levelData.GetTiles().Count > 15 && launchPadsInsideLevel && landingPadsInsideLevel) tutorialStage++;
                break;
            case 1:
                if (Vector2.Distance(levelData.GetLandPad().GetPosition(), new Vector2(8.8f, 0.7f)) < 0.1 && !LevelEditorCursor.IsCurrentlyMovingObject()) tutorialStage++;
                break;
            case 3:
                var doorObj = levelData.GetObjectAtPosition(new Vector2(4.5f, 2.5f)); //look for the door
                if(doorObj is LevelEntity && !LevelEditorCursor.IsCurrentlyMovingObject())
                {
                    var entity = (LevelEntity)doorObj;
                    if(entity.resourceName == "Door/Door")
                    {
                        doorEntity = entity;
                        tutorialStage++;
                    }
                }
                break;
            case 4:
                var sensorObj = levelData.GetEntityAtArea(new Vector2(2.5f, 0.6f), 0.5f); //look for the landing pad sensor
                if (sensorObj is LevelEntity && !LevelEditorCursor.IsCurrentlyMovingObject())
                {
                    var entity = (LevelEntity)sensorObj;

                    if (entity.resourceName == "Ship Pads/Ship Sensor Pad")
                    {
                        sensorEntity = entity;
                        tutorialStage++;
                    }
                }
                break;
            case 5:
                if(doorEntity == null)
                {
                    tutorialStage = 3;
                    return;
                }
                if (sensorEntity == null)
                {
                    tutorialStage = 4;
                    return;
                }

                if (sensorEntity.logicTarget == doorEntity) tutorialStage++;
                break;
        }
    }

    public void MoveToNextStage()
    {
        tutorialStage++;

        nextStageButton.interactable = false;
    }

    private void Update()
    {
        if (enableTutorial)
        {
            if (tutorialStage == 1)
            {
                Vector2 realLandPadPos = theRealLandPad.transform.position;
                Vector2 haloLandPadPos = landHalo.transform.position;

                followWorktablePosition = Vector2.Lerp(realLandPadPos, haloLandPadPos, 0.5f);
                followWorktablePosition.y = Mathf.Min(realLandPadPos.y, haloLandPadPos.y) - 1;
            }

            if (shouldFollowWorktablePosition)
            {
                rectTransform.position = Vector2.Lerp(rectTransform.position, MainCameraController.Singletron.selfCamera.WorldToScreenPoint(followWorktablePosition), 0.3f);
            }
            else
            {
                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, new Vector2(0, 239.9f), 0.2f);
            }

            if (lastTutorialStage != tutorialStage)
            {
                UpdateTutorialText();
            }
            lastTutorialStage = tutorialStage;
        }
        else
        {
            HideTutorialText();
        }
    }
}
