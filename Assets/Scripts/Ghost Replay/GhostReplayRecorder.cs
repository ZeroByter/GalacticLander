using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GhostReplayRecorder : MonoBehaviour {
    public static GhostReplayRecorder Singleton;
    public static int NextGhostId = 0;

    public static void AddNonPlayerSnapshot(GhostReplayNonPlayerSnapshot snapshot)
    {
        if (Singleton == null) return;
        if (Singleton.newNonPlayerSnapshotsBuffer.ContainsKey(snapshot.id)) return;
        
        Singleton.newNonPlayerSnapshotsBuffer.Add(snapshot.id, snapshot);
    }

    public static void AddPlayerSnapshot(GhostReplayPlayerSnapshot snapshot)
    {
        if (Singleton == null) return;
        if (Singleton.newPlayerSnapshotsBuffer.ContainsKey(snapshot.id)) return;

        Singleton.newPlayerSnapshotsBuffer.Add(snapshot.id, snapshot);
    }

    public static void AddInitialPlayerData(int id, int skinIndex, string playerName)
    {
        if (Singleton == null) return;

        var currentReplay = Singleton.GetCurrentGhostReplay();

        currentReplay.shipGhostReplayIds.Add(id);

        currentReplay.shipsSkinIndex.Add(id, skinIndex);
        currentReplay.playerShipNames.Add(id, playerName);
    }

    private GhostReplay ghostReplay;

    private float lastRecordTime;
    private float delay = 0.25f;

    private bool ignoreCoopLevel = false;
    
    private Dictionary<int, GhostReplayNonPlayerSnapshot> newNonPlayerSnapshotsBuffer = new Dictionary<int, GhostReplayNonPlayerSnapshot>();
    private Dictionary<int, GhostReplayPlayerSnapshot> newPlayerSnapshotsBuffer = new Dictionary<int, GhostReplayPlayerSnapshot>();

    private void Awake() {
        Singleton = this;

        ghostReplay = new GhostReplay();
        ghostReplay.levelName = Path.GetFileNameWithoutExtension(LevelLoader.GetLevelDirectory());
    }

    private void Start() {
        if(LevelLoader.Singletron != null) {
            if (LevelLoader.Singletron.levelData.IsCoopLevel() && !NetworkingManager.CurrentLobbyValid) { //if we are not in a lobby and we are playing a co-op level
                ignoreCoopLevel = true;
                Destroy(gameObject);
                return;
            }
        }
    }

    private void Update() {
        if(Time.time > lastRecordTime + delay) {
            lastRecordTime = Time.time;

            if(newPlayerSnapshotsBuffer.Count > 0)
            {
                GhostReplaySnapshot newSnapshot = new GhostReplaySnapshot();
                newSnapshot.time = Time.timeSinceLevelLoad;

                foreach (var nonPlayerSnapshot in newNonPlayerSnapshotsBuffer.Values)
                {
                    newSnapshot.nonPlayerSnapshots.Add(nonPlayerSnapshot.id, nonPlayerSnapshot);
                }
                newNonPlayerSnapshotsBuffer.Clear();

                foreach (var playerSnapshot in newPlayerSnapshotsBuffer.Values)
                {
                    newSnapshot.playerSnapshots.Add(playerSnapshot.id, playerSnapshot);
                }
                newPlayerSnapshotsBuffer.Clear();

                ghostReplay.snapshots.Add(newSnapshot);
            }
        }
    }

    private void OnDestroy() {
        if (!GhostReplay.Enabled || ignoreCoopLevel || Time.time == 0) return;

        string saveDirectory = Application.persistentDataPath + "/Level Replays/";
        string savePath = saveDirectory + ghostReplay.levelName + ".replay";

        if (!Directory.Exists(saveDirectory)) {
            Directory.CreateDirectory(saveDirectory);
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(savePath, FileMode.Create);
        bf.Serialize(file, ghostReplay);
        file.Close();
    }

    private T DeepClone<T>(T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }

    public GhostReplay GetCurrentGhostReplay()
    {
        return ghostReplay;
    }

    public GhostReplay GetCurrentGhostReplayDeepCopy()
    {
        return DeepClone(ghostReplay);
    }
}
