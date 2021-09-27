using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GhostReplayPlayback : MonoBehaviour {
    public static GhostReplayPlayback Singleton;
    public static float GhostFullAlpha = 0.4f;

    [Header("The death camera which will show the playback")]
    public Camera deathPlaybackCamera;
    [Header("List of ghost replay player ships")]
    [SerializeField]
    private GhostReplayShipController[] ghostShips;

    private GhostReplay replay = null;

    private Dictionary<int, SpriteRenderer> nonPlayerSnapshotsGameObjects = new Dictionary<int, SpriteRenderer>();
    private List<SpriteRenderer> hideRenderers = new List<SpriteRenderer>();

    private enum PlaybackTime
    {
        SinceLevelLoad,
        Custom
    }
    private PlaybackTime playbackTime = PlaybackTime.SinceLevelLoad;
    private float customPlaybackTime;
    
    private bool loop = false;

    private float GetPlaybackTime()
    {
        if(playbackTime == PlaybackTime.SinceLevelLoad)
        {
            return Time.timeSinceLevelLoad;
        }
        else
        {
            return Time.time - customPlaybackTime;
        }
    }

    private void Awake()
    {
        GhostFullAlpha = 0.4f;
        Singleton = this;
    }

    private void HideAllImmediately()
    {
        foreach (var ship in ghostShips)
        {
            ship.HideImmediately();
        }
    }

    private void HideAll()
    {
        foreach (var ship in ghostShips)
        {
            ship.Hide(0.05f);
        }
    }

    private void Start() {
        if (!GhostReplay.Enabled) return;

        hideRenderers.Clear();
        replay = GhostReplay.GetReplay(Path.GetFileNameWithoutExtension(LevelLoader.GetLevelDirectory()));

        if(replay == null || replay.playerShipNames == null) {
            HideAllImmediately();
            return;
        }

        if (replay.shipGhostReplayIds != null)
        {
            var firstSnapshot = replay.GetFirstSnapshot();
            
            int firstId = replay.shipGhostReplayIds[0];
            
            if (replay.shipsSkinIndex.Count == 1)
            {
                ghostShips[0].Setup(replay.shipsSkinIndex[firstId], "", firstSnapshot);
                ghostShips[1].HideImmediately();
            }
            else
            {
                int secondId = replay.shipGhostReplayIds[1];
                ghostShips[0].Setup(replay.shipsSkinIndex[firstId], replay.playerShipNames[firstId], firstSnapshot);
                ghostShips[1].Setup(replay.shipsSkinIndex[secondId], replay.playerShipNames[secondId], firstSnapshot);
            }
        }
        else
        {
            HideAll();
        }
    }

    public static Color LerpAlphaToZero(Color color, float lerp = 0.15f) {
        return new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 0, lerp));
    }

    public static Color LerpAlphaToFull(Color color) {
        float lerp = 0.3f;

        return new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, GhostFullAlpha, lerp));
    }
    
    private void Update() {
        if (replay == null || replay.playerShipNames == null)
        {
            HideAllImmediately();

            return;
        }

        float playbackTime = GetPlaybackTime();

        GhostReplaySnapshot currentSnapshot;
        GhostReplaySnapshot nextSnapshot;
        GhostReplaySnapshot lastSnapshot = replay.GetLastSnapshot();
        replay.GetSnapshots(playbackTime, out currentSnapshot, out nextSnapshot);

        if (currentSnapshot == null) return;

        if (nextSnapshot != null && playbackTime <= lastSnapshot.time) {
            float lerp = Mathf.InverseLerp(currentSnapshot.time, nextSnapshot.time, playbackTime);

            for (int i = 0; i < replay.playerShipNames.Count; i++)
            {
                int shipId = replay.shipGhostReplayIds[i];
                if (!currentSnapshot.playerSnapshots.ContainsKey(shipId)) continue;

                ghostShips[i].UpdatePerSnapshot(lerp, currentSnapshot.playerSnapshots[shipId], nextSnapshot.playerSnapshots[shipId]);
            }

            if (currentSnapshot.nonPlayerSnapshots != null)
            {
                foreach (var pair in currentSnapshot.nonPlayerSnapshots)
                {
                    SpriteRenderer renderer = GetNonPlayerSnapshotGameObject(pair.Value.id, pair.Value.spriteResource);

                    if (nextSnapshot.nonPlayerSnapshots.ContainsKey(pair.Key))
                    {
                        var nextNonPlayerSnapshot = nextSnapshot.nonPlayerSnapshots[pair.Key];

                        renderer.transform.position = Vector2.Lerp(pair.Value.GetPosition(), nextNonPlayerSnapshot.GetPosition(), lerp);
                        renderer.transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(pair.Value.rotation, nextNonPlayerSnapshot.rotation, lerp));

                        renderer.color = LerpAlphaToFull(renderer.color);
                    }
                    else
                    {
                        renderer.transform.position = pair.Value.GetPosition();
                        renderer.transform.eulerAngles = new Vector3(0, 0, pair.Value.rotation);
                        renderer.color = LerpAlphaToZero(renderer.color, lerp);
                        hideRenderers.Add(renderer);
                    }
                }
            }

            foreach (var renderer in hideRenderers)
            {
                renderer.color = LerpAlphaToZero(renderer.color, lerp);
            }
        } else {
            float lerp = 0.05f;

            HideAll();

            if (currentSnapshot.nonPlayerSnapshots != null)
            {
                foreach (var pair in currentSnapshot.nonPlayerSnapshots)
                {
                    SpriteRenderer gameObject = GetNonPlayerSnapshotGameObject(pair.Value.id, pair.Value.spriteResource);

                    gameObject.transform.position = pair.Value.GetPosition();
                    gameObject.transform.eulerAngles = new Vector3(0, 0, pair.Value.rotation);
                    gameObject.color = LerpAlphaToZero(gameObject.color, lerp);
                }
            }

            if (loop && GetPlaybackTime() > lastSnapshot.time + 0.8f)
            {
                customPlaybackTime = Time.time;
            }
        }

        List<Vector3> allShipPositions = new List<Vector3>();

        for (int i = 0; i < ghostShips.Length; i++)
        {
            GhostReplayShipController shipController = ghostShips[i];

            if (shipController.GetBodyRendererAlpha() >= GhostFullAlpha / 2)
            {
                allShipPositions.Add(shipController.transform.position);
            }
        }

        if(allShipPositions.Count > 0)
        {
            Vector3 averagePosition = Vector3.zero;
            foreach(var position in allShipPositions)
            {
                averagePosition += position;
            }
            averagePosition /= allShipPositions.Count;

            deathPlaybackCamera.transform.position = averagePosition + new Vector3(0, 0, -30);
            deathPlaybackCamera.orthographicSize = GetRequiredSize(deathPlaybackCamera.transform.InverseTransformPoint(averagePosition), allShipPositions, 3f, 2.5f);
        }
    }

    private float GetRequiredSize(Vector3 startPosition, List<Vector3> positions, float screenEdgeBuffer, float minSize)
    {
        float size = 0;

        foreach (Vector3 position in positions)
        {
            Vector3 targetLocalPos = deathPlaybackCamera.transform.InverseTransformPoint(position);
            Vector3 desiredPosToTarget = targetLocalPos - startPosition;
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / deathPlaybackCamera.aspect);
        }

        size += screenEdgeBuffer;
        size = Mathf.Max(size, minSize);

        return size;
    }

    public void PlayCustomReplay(GhostReplay replayData, float fullAlpha, bool loop, bool switchToGhostReplayLayer)
    {
        playbackTime = PlaybackTime.Custom;
        customPlaybackTime = Time.time;
        hideRenderers.Clear();
        replay = replayData;

        int firstId = replay.shipGhostReplayIds[0];

        if (replayData.shipsSkinIndex.Count == 1)
        {
            ghostShips[0].Setup(replayData.shipsSkinIndex[firstId], "", replayData.GetFirstSnapshot(), switchToGhostReplayLayer);
            ghostShips[1].HideImmediately();
        }
        else
        {
            for (int i = 0; i < ghostShips.Length; i++)
            {
                int shipId = replay.shipGhostReplayIds[i];

                ghostShips[i].Setup(replayData.shipsSkinIndex[shipId], replayData.playerShipNames[shipId], replayData.GetFirstSnapshot(), switchToGhostReplayLayer);
            }
        }

        GhostFullAlpha = fullAlpha;
        this.loop = loop;
    }

    private SpriteRenderer GetNonPlayerSnapshotGameObject(int id, string spriteResource)
    {
        SpriteRenderer gameObject = null;

        if(!nonPlayerSnapshotsGameObjects.TryGetValue(id, out gameObject))
        {
            var newGameObject = new GameObject();
            gameObject = newGameObject.AddComponent<SpriteRenderer>();
            gameObject.sprite = Resources.Load<Sprite>(spriteResource);
            nonPlayerSnapshotsGameObjects.Add(id, gameObject);
        }

        return gameObject;
    }
}
