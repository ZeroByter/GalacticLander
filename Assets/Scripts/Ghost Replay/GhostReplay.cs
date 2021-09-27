using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using ComponentHealth = ShipComponentController.ComponentHealth;

[Serializable]
public class GhostReplaySnapshot {
    public float time;

    public Dictionary<int, GhostReplayPlayerSnapshot> playerSnapshots = new Dictionary<int, GhostReplayPlayerSnapshot>();
    public Dictionary<int, GhostReplayNonPlayerSnapshot> nonPlayerSnapshots = new Dictionary<int, GhostReplayNonPlayerSnapshot>();
}

[Serializable]
public class GhostReplayPlayerSnapshot
{
    public int id;

    public float x;
    public float y;
    public float rotation;

    public ComponentHealth leftEngine = ComponentHealth.Intact;
    public ComponentHealth leftLeg = ComponentHealth.Intact;
    public ComponentHealth rightLeg = ComponentHealth.Intact;
    public ComponentHealth rightEngine = ComponentHealth.Intact;

    public bool leftEngineOn;
    public bool rightEngineOn;

    public Vector2 GetPosition()
    {
        return new Vector2(x, y);
    }
}

[Serializable]
public class GhostReplayNonPlayerSnapshot
{
    public int id;

    public float x;
    public float y;
    public float rotation;

    public string spriteResource;

    public GhostReplayNonPlayerSnapshot(int id, float x, float y, float rotation, string spriteResource)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        this.rotation = rotation;
        this.spriteResource = spriteResource;
    }

    public Vector2 GetPosition()
    {
        return new Vector2(x, y);
    }
}

[Serializable]
public class GhostReplayCarriedCrateSnapshot : GhostReplayNonPlayerSnapshot
{
    public GhostReplayCarriedCrateSnapshot(int id, float x, float y, float rotation, string spriteResource) : base(id, x, y, rotation, spriteResource) { }


}

[Serializable]
public class GhostReplay {
    public static bool Enabled {
        get {
            return PlayerPrefs.GetInt("enableGhostReplay", 1) == 1;
        }
        set {
            PlayerPrefs.SetInt("enableGhostReplay", value ? 1 : 0);
        }
    }

    public string levelName;

    public Dictionary<int, int> shipsSkinIndex = new Dictionary<int, int>();
    public Dictionary<int, string> playerShipNames = new Dictionary<int, string>();
    
    public List<GhostReplaySnapshot> snapshots = new List<GhostReplaySnapshot>();

    public List<int> shipGhostReplayIds = new List<int>();

    [NonSerialized]
    private int nextGhostReplayId;

    public GhostReplaySnapshot GetFirstSnapshot()
    {
        if (snapshots.Count == 0) return null;
        return snapshots[0];
    }

    public GhostReplaySnapshot GetLastSnapshot() {
        if (snapshots.Count == 0) return null;
        return snapshots[snapshots.Count - 1];
    }

    public void GetSnapshots(float time, out GhostReplaySnapshot currentSnapshot, out GhostReplaySnapshot nextSnapshot) {
        currentSnapshot = null;
        nextSnapshot = null;

        if (snapshots.Count == 0) {
            return;
        }

        for(int i = 0; i < snapshots.Count; i++) {
            GhostReplaySnapshot snapshot = snapshots[i];

            if(currentSnapshot == null || snapshot.time > currentSnapshot.time && snapshot.time <= time) {
                currentSnapshot = snapshot;
                if(i + 1 < snapshots.Count) {
                    nextSnapshot = snapshots[i + 1];
                } else {
                    nextSnapshot = null;
                }
            }
        }

        return;
    }

    public int GetNewGhostReplayId()
    {
        return nextGhostReplayId++;
    }

    public static GhostReplay GetReplay(string levelName) {
        string saveDirectory = Application.persistentDataPath + "/Level Replays/";
        string savePath = saveDirectory + levelName + ".replay";

        if (!Directory.Exists(saveDirectory)) return null;

        if (File.Exists(savePath)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(savePath, FileMode.Open);

            GhostReplay replay = (GhostReplay)bf.Deserialize(file);
            file.Close();

            return replay;
        } else {
            return null;
        }
    }
}
