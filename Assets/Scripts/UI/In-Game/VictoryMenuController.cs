using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.SceneManagement;
using Steamworks;
using System.IO;
using UnityEngine.UI;

public class VictoryMenuController : MonoBehaviour {
    [Serializable]
    private class LevelGlobalBestTime
    {
        public float bestScore;
        public float bestTime;
    }

    public static VictoryMenuController Singletron;
    public static bool LevelCompleted;

    [Header("Transition component")]
    public CanvasBlurTransition transition;
    [Header("TMPro stuff")]
    public TMP_Text totalTime;
    public TMP_Text totalScore;
    public TMP_Text timeCompletedText;
    public TMP_Text shipStatusText;
    [Header("Stats to beat container")]
    public RectTransform statsToBeatRectTransform;
    public LerpCanvasGroup statsToBeatLerpCanvasGroup;
    public TMP_Text statsToBeatBestTimeText;
    public TMP_Text statsToBeatBestScoreText;
    [Header("Next level button")]
    public GameObject nextLevelButton;
    [Header("The player's ship stuff")]
    public ShipComponentController leftEngine;
    public ShipComponentController leftLeg;
    public ShipComponentController rightLeg;
    public ShipComponentController rightEngine;

    [HideInInspector]
    public bool showNextLevelButton;
    [Header("The current level number/id")]
    public int currentLevelNumber;
    
    [Header("Victory time + score")]
    public float victoryTime;
    public float victoryScore;

    [HideInInspector]
    public bool isOpen {
        get {
            return transition.isOpen;
        }
    }

    private Rect statsToBeatRect;

    private bool showStatsToBeat = false;

    private void Awake() {
        Singletron = this;

        statsToBeatRect = statsToBeatRectTransform.rect;
    }

    private void Start() {
        LevelCompleted = false; 
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.F6) && IsUserDebugger.GetIsUserDebugger()) {
            GotoNextLevel();
        }

        if (showStatsToBeat)
        {
            statsToBeatRectTransform.sizeDelta = new Vector2(320, statsToBeatRectTransform.sizeDelta.y);
            statsToBeatLerpCanvasGroup.target = 1;
        }
        else
        {
            statsToBeatRectTransform.sizeDelta = new Vector2(0, statsToBeatRectTransform.sizeDelta.y);
            statsToBeatLerpCanvasGroup.target = 0;
        }
    }

    public static string GetShipComponentHealth(ShipComponentController component) {
        if (component != null) {
            string componentHealth = component.GetHealth().ToString().ToLower();
            if(componentHealth == "intact") {
                componentHealth = string.Format("<color=#0f0>{0}</color>", componentHealth);
            } else if (componentHealth == "broken") {
                componentHealth = string.Format("<color=#ff6e00>{0}</color>", componentHealth);
            } else {
                componentHealth = string.Format("<color=#f00>{0}</color>", componentHealth);
            }
            return componentHealth;
        } else {
            return "<color=#f00>destroyed</color>";
        }
    }

    private static float ShipComponentHealthToScore(ShipComponentController component) {
        if (component != null) {
            string componentHealth = component.GetHealth().ToString().ToLower();
            if (componentHealth == "intact") {
                return -2;
            } else if (componentHealth == "broken") {
                return -1;
            } else {
                return 1;
            }
        } else {
            return 1;
        }
    }

    private static string FloatToString(float f) {
        if (f > 0) {
            return "+" + f.ToString();
        } else {
            return f.ToString();
        }
    }

    public static void OpenMenu() {
        if (DeathMenuController.Singletron.isOpen) return;
        if (Singletron.transition.isOpen) return;

        Singletron.statsToBeatRect.width = 0;
        Singletron.statsToBeatLerpCanvasGroup.target = 0;

        Singletron.StartCoroutine(Singletron.GetLevelBestStatistics());

        LevelCompleted = true;
        Singletron.transition.OpenMenu();

        float leftEngineScore = ShipComponentHealthToScore(Singletron.leftEngine);
        float leftLegScore = ShipComponentHealthToScore(Singletron.leftLeg);
        float rightLegScore = ShipComponentHealthToScore(Singletron.rightLeg);
        float rightEngineScore = ShipComponentHealthToScore(Singletron.rightEngine);

        Singletron.totalTime.text = string.Format("Total time: <b>{0}</b>", Math.Round(FirstMovementTimer.GetTimeSinceFirstMovement(), 4));
        Singletron.timeCompletedText.text = string.Format("Level completed in: {0} seconds", Math.Round(FirstMovementTimer.GetTimeSinceFirstMovement(), 4));
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(string.Format("Left engine: {0} ({1})", GetShipComponentHealth(Singletron.leftEngine), FloatToString(leftEngineScore)));
        sb.AppendLine(string.Format("Left leg: {0} ({1})", GetShipComponentHealth(Singletron.leftLeg), FloatToString(leftLegScore)));
        sb.AppendLine(string.Format("Right leg: {0} ({1})", GetShipComponentHealth(Singletron.rightLeg), FloatToString(rightLegScore)));
        sb.AppendLine(string.Format("Right engine: {0} ({1})", GetShipComponentHealth(Singletron.rightEngine), FloatToString(rightEngineScore)));
        Singletron.shipStatusText.text = sb.ToString();

        float score = FirstMovementTimer.GetTimeSinceFirstMovement() + leftEngineScore + leftLegScore + rightLegScore + rightEngineScore;
        Singletron.victoryScore = score;
        Singletron.victoryTime = FirstMovementTimer.GetTimeSinceFirstMovement();

        Singletron.totalScore.text = "Score: <b>" + Math.Round(score, 4) + "</b>";

        if (Singletron.showNextLevelButton) {
            if (LevelLoader.IsPlayingEventLevel()) {
                bool nextLevel = true;
                
                GameEvent currentEvent = GameEvents.GetCurrentEvent();

                if (NetworkingManager.CurrentLobbyValid) { //if we are in a coop level
                    nextLevel = GameEvents.CurrentEventCoopProgression + 1 < currentEvent.numberOfLevels;
                } else {
                    nextLevel = GameEvents.CurrentEventProgression + 1 < currentEvent.numberOfLevels;
                }

                Singletron.nextLevelButton.SetActive(nextLevel);
            } else {
                string levelPrefix = "";
                if (LevelLoader.GetLevelDirectory().StartsWith("sp")) {
                    levelPrefix = "sp";
                } else {
                    levelPrefix = "mp";
                }

                TextAsset nextLevel = Resources.Load<TextAsset>(levelPrefix + (Singletron.currentLevelNumber + 1));
                Singletron.nextLevelButton.SetActive(nextLevel != null);
            }
        } else {
            Singletron.nextLevelButton.SetActive(false);
        }

        bool isPlayingEventLevel = false;

        //check achievements here
        //check if we landed with no landing legs
        if (leftLegScore == 1 && rightLegScore == 1) SteamCustomUtils.SetAchievement("NO_LEGS");

        #region Updating current event progression
        if (LevelLoader.Singletron != null) { //if the level loader exists and there is an event currently running
            GameEvent currentEvent = GameEvents.GetCurrentEvent();

            //we check if we are playing an event level
            if (LevelLoader.IsPlayingEventLevel()) { //if we are indeed playing an event level...
                isPlayingEventLevel = true;

                //we advance the level progression
                if (NetworkingManager.CurrentLobbyValid) {
                    GameEvents.CurrentEventCoopProgression = LevelLoader.Singletron.GetCurrentEventLevelNumber() + 1;
                } else {
                    GameEvents.CurrentEventProgression = LevelLoader.Singletron.GetCurrentEventLevelNumber() + 1;
                }

                if (GameEvents.CurrentEventCoopProgression >= currentEvent.numberOfLevels || GameEvents.CurrentEventProgression >= currentEvent.numberOfLevels) {
                    Singletron.StartCoroutine(GameAchievementsManager.SetGameAchievement(currentEvent.name));
                }
            }
        }
        #endregion

        //here we update level progress
        if (!LevelLoader.PlayTestingLevel) {
            //setting up basic variables
            string websiteStatsLevelName = "";
            string levelName = LevelLoader.GetLevelDirectory();
            LevelData currentLevelData = LevelLoader.Singletron.levelData;

            if (currentLevelData.workshopId != 0) { //if we left a workshop level
                SteamUGC.StopPlaytimeTrackingForAllItems();
            }

            LevelProgress.LevelType levelType = LevelProgress.LevelType.Game;

            if (levelName.EndsWith(".level")) { //file is external/workshop (since workshop files are stored externally
                levelType = LevelProgress.LevelType.External;
            } else { //otherwise file is game-provided
                levelType = LevelProgress.LevelType.Game;
            }

            //here we actually update the progress arccordingly
            LevelProgress newProgress = LevelProgressCounter.GetProgress(levelType, levelName, LevelLoader.GravityEnabled);

            if (newProgress == null) { //if the progress file doesn't exist we create a new one
                newProgress = new LevelProgress();

                newProgress.gravityEnabled = LevelLoader.GravityEnabled;

                if (levelType == LevelProgress.LevelType.Game) {
                    newProgress.SetAssetLevel(levelName);

                    if (LevelLoader.IsPlayingEventLevel())
                    {
                        websiteStatsLevelName = levelName;// GameEvents.NormalLevelNameToEventName(levelName);
                    }
                    else
                    {
                        websiteStatsLevelName = levelName;
                    }
                }
                if (levelType == LevelProgress.LevelType.External) {
                    if (currentLevelData.workshopId != 0) { //if this level is a workshop level
                        newProgress.SetExternalLevel("workshop:" + currentLevelData.workshopId);
                        websiteStatsLevelName = currentLevelData.workshopId.ToString();
                    } else {
                        websiteStatsLevelName = "";
                        newProgress.SetExternalLevel(levelName);
                    }
                }
            }

            newProgress.lastPlayedTime = Singletron.victoryTime;
            newProgress.lastPlayedScore = Singletron.victoryScore;
            if (newProgress.lastPlayedTime <= newProgress.bestTime || newProgress.timesCompleted == 0) newProgress.bestTime = newProgress.lastPlayedTime;
            if (newProgress.lastPlayedScore <= newProgress.bestScore || newProgress.timesCompleted == 0) newProgress.bestScore = newProgress.lastPlayedScore;

            //update stats to website
            if (websiteStatsLevelName != "")
            {
                WebsiteNetworking.PostStatistic("bestLevelTime", websiteStatsLevelName, newProgress.bestTime.ToString());
                WebsiteNetworking.PostStatistic("bestLevelScore", websiteStatsLevelName, newProgress.bestScore.ToString());
            }

            newProgress.timesCompleted++;

            LevelProgressCounter.UpdateOrCreateProgress(newProgress);
        }

        if (!isPlayingEventLevel && !LevelLoader.PlayTestingLevel)
        {
            //assign level-related stats here
            LevelProgress levelProgress = LevelProgressCounter.GetProgress(LevelProgress.LevelType.Game, LevelLoader.GetLevelDirectory(), LevelLoader.GravityEnabled);
            if (levelProgress != null)
            { //if the level is valid
                if (LevelLoader.GetLevelDirectory().StartsWith("sp"))
                {
                    SteamCustomUtils.AddStat("SP_PLAYED");
                }
                else
                {
                    SteamCustomUtils.AddStat("COOP_PLAYED");
                }

                if (leftEngineScore == -2 && leftLegScore == -2 && rightLegScore == -2 && rightEngineScore == -2)
                { //if the players ship is completely intact
                    SteamCustomUtils.AddStat("LEVELS_PLAYED_HARMLESS");
                }
                else
                {
                    SteamCustomUtils.SetStat("LEVELS_PLAYED_HARMLESS", 0);
                }

                if (!LevelLoader.GravityEnabled)
                {
                    SteamCustomUtils.AddStat("LEVELS_PLAYED_NO_GRAVITY");
                }
            }
        }

        //check if we played all levels
        int spLevelsPlayed;
        int coopLevelsPlayed;
        LevelProgressCounter.CountGameLevelsPlayed(out spLevelsPlayed, out coopLevelsPlayed);

        if (spLevelsPlayed >= LevelSelectionManager.TotalSingleplayerLevels) SteamCustomUtils.SetAchievement("ALL_SP_PLAYED");
        if (coopLevelsPlayed >= LevelSelectionManager.TotalCoopLevels) SteamCustomUtils.SetAchievement("ALL_COOP_PLAYED");
    }

    public void GotoNextLevel() {
        if(SteamManager.Initialized && NetworkingManager.CurrentLobbyValid) { //if we are in coop
            CoopVoteController.StartVote(CoopVoteController.VoteType.GotoNextLevel);
        } else {
            if(LevelLoader.IsPlayingEventLevel()) { //if there is an active event
                GameEvent currentEvent = GameEvents.GetCurrentEvent();

                string nextLevelName = string.Format("{0}/{0}_", currentEvent.name);
                
                if (NetworkingManager.CurrentLobbyValid) { //if we are in a coop level
                    nextLevelName += "mp";
                    nextLevelName += Mathf.Min(currentEvent.numberOfLevels, GameEvents.CurrentEventCoopProgression).ToString();
                } else {
                    nextLevelName += "sp";
                    nextLevelName += Mathf.Min(currentEvent.numberOfLevels, GameEvents.CurrentEventProgression).ToString(); //check if this works and finish code
                }

                LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.Game, nextLevelName);
            } else {
                var fileName = Path.GetFileNameWithoutExtension(LevelLoader.GetLevelDirectory());

                string levelPrefix = "sp";
                if (fileName.StartsWith("mp")) {
                    levelPrefix = "mp";
                }
                if(fileName.StartsWith("osp") || fileName.StartsWith("omp"))
                {
                    levelPrefix = "o" + levelPrefix;
                }

                LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.Game, levelPrefix + (currentLevelNumber + 1));
            }

            SceneManager.LoadScene("Game Level");
        }
    }

    private IEnumerator GetLevelBestStatistics()
    {
        string levelName = LevelLoader.GetLevelDirectory();
        if (LevelLoader.IsPlayingEventLevel())
        {
            levelName = GameEvents.NormalLevelNameToEventName(levelName);
        }

        CoroutineWithData cd = new CoroutineWithData(this, WebsiteNetworking.GetSingletron().GetLevelBestStatisticsData(levelName));
        yield return cd.coroutine;
        var data = cd.result.ToString();

        if (data != "false")
        {
            var timeData = JsonUtility.FromJson<LevelGlobalBestTime>(data);

            if(timeData != null)
            {
                if(Math.Round(timeData.bestTime, 3) + Math.Round(timeData.bestScore, 3) > 0)
                {
                    statsToBeatBestTimeText.text = $"Best Global Time: <b>{Math.Round(timeData.bestTime, 3)}</b>";
                    statsToBeatBestScoreText.text = $"Best Global Score: <b>{Math.Round(timeData.bestScore, 3)}</b>";
                    showStatsToBeat = true;

                    statsToBeatRectTransform.sizeDelta = new Vector2(320, statsToBeatRectTransform.sizeDelta.y);
                    statsToBeatLerpCanvasGroup.target = 1;

                    //yield return new WaitForSeconds(0.1f);

                    //LayoutRebuilder.ForceRebuildLayoutImmediate(statsToBeatRectTransform);
                }
                else
                {
                    showStatsToBeat = false;
                }
            }
            else
            {
                showStatsToBeat = false;
            }
        }
        else
        {
            showStatsToBeat = false;
        }
    }
}
