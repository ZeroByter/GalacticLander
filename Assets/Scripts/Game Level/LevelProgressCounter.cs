using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelProgress {
    public DateTime lastPlayed;
    public float lastPlayedTime;
    public float lastPlayedScore;
    public float bestTime;
    public float bestScore;
    public int timesCompleted;
    public int triesBeforeSuccess;
    public bool gravityEnabled = true;

    public enum LevelType { Game, External }
    private LevelType levelType;
    private string assetName;
    private string pathName;
    private int workshopId;

    public LevelProgress() {
        UpdateLastPlayed();
    }

    public void SetAssetLevel(string assetName) {
        levelType = LevelType.Game;
        this.assetName = assetName;
    }

    public void SetExternalLevel(string path) {
        levelType = LevelType.External;
        pathName = path;
    }

    public string GetLevelIdentifier() {
        if(levelType == LevelType.Game) {
            return assetName;
        }
        if (levelType == LevelType.External) {
            return pathName;
        }

        return "";
    }

    public LevelType GetLevelType() {
        return levelType;
    }

    public void UpdateLastPlayed() {
        lastPlayed = DateTime.Now;
    }
}

public class LevelProgressCounter : MonoBehaviour {
    public static LevelProgressCounter Singletron;
    public static List<LevelProgress> LevelProgressList;

    private static string GetFilePath() {
        return Application.persistentDataPath + "/Level Progress/levelsprogress.dat";
    }

    public static void UpdateFile() {
        if (!File.Exists(GetFilePath())) LevelProgressList = new List<LevelProgress>(); //if the list is null, we create a new blank one

        if (!Directory.Exists(Path.GetDirectoryName(GetFilePath()))) Directory.CreateDirectory(Path.GetDirectoryName(GetFilePath()));
        
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(GetFilePath(), FileMode.OpenOrCreate);
        bf.Serialize(file, LevelProgressList);
        file.Close();
    }

    public static void LoadFile() {
        if (!File.Exists(GetFilePath())) UpdateFile(); //if the file doesn't exist, then it means the list has to be empty, therefore we call updatefile and create a new blank binary file

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(GetFilePath(), FileMode.Open);
        try {
            LevelProgressList = (List<LevelProgress>)bf.Deserialize(file);
        } catch {
            LevelProgressList = new List<LevelProgress>(); //file is corrupt, have to rewrite
            bf.Serialize(file, LevelProgressList);
        }
        file.Close();

        print(string.Format("loaded progress file, list has {0} entries", LevelProgressList.Count));
    }

    public static LevelProgress GetProgress(LevelProgress.LevelType type, string identifier, bool gravityEnabled) {
        if (LevelProgressList == null) LoadFile();

        foreach (LevelProgress level in LevelProgressList) {
            if(level.GetLevelType() == type && level.GetLevelIdentifier() == identifier && level.gravityEnabled == gravityEnabled) return level; //if the level is the same type as the one we are searching for and if the identifier is the same as well, we return the level
        }

        return null; //didnt find a level
    }

    public static void UpdateOrCreateProgress(LevelProgress progress) {
        if (LevelProgressList == null) UpdateFile(); //if the list is null, we call UpdateFile. this means if there is a file we load it, if not we simply get a new blank list object, all is well

        bool updatedList = false;
        for(int i = 0; i < LevelProgressList.Count; i++) {
            LevelProgress arrayProgress = LevelProgressList[i];

            if(arrayProgress.GetLevelType() == progress.GetLevelType() && arrayProgress.GetLevelIdentifier() == progress.GetLevelIdentifier()) {
                updatedList = true;
                LevelProgressList[i] = progress;
            }
        }

        if (!updatedList) {
            LevelProgressList.Add(progress);
        }

        UpdateFile();
    }

    public static bool GetAverageScore(out float bestAverageScore, out float bestAverageTime) {
        LoadFile();

        int levelsCount = 0;

        float averageS = 0;
        float averageT = 0;

        foreach(LevelProgress progress in LevelProgressList) {
            if(progress.lastPlayedTime > 0 && progress.lastPlayedScore > 0) {
                levelsCount++;

                averageS += progress.bestScore;
                averageT += progress.bestTime;
            }
        }

        bestAverageScore = averageS / levelsCount;
        bestAverageTime = averageT / levelsCount;

        return levelsCount > 0;
    }

    private void Awake() {
        if (Singletron != null) {
            Destroy(gameObject);
            return;
        }

        Singletron = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void CountGameLevelsPlayed(out int spPlayed, out int coopPlayed) {
        spPlayed = 0;
        coopPlayed = 0;

        foreach(LevelProgress progress in LevelProgressList) {
            if(progress.timesCompleted > 0) {
                if (progress.GetLevelIdentifier().StartsWith("sp")) spPlayed++;
                if (progress.GetLevelIdentifier().StartsWith("mp")) coopPlayed++;
            }
        }
    }
}
