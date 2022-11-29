using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Steamworks;
using SourceConsole;

public class CinematicLevelCamera : MonoBehaviour {
    public static CinematicLevelCamera Singleton;

    [ConCommand]
    public static void CinematicLevelCamera_TakeScreenshot(bool cinematic)
    {
        if (Singleton == null) return;

        Singleton.BeginTakeScreenshot(cinematic);
    }

    public LayerMask everythingMask;
    public LayerMask cinematicMask;

    private Camera screenshotCamera;

    private bool cinematicScreenshot;

    private void Awake() {
        Singleton = this;

        screenshotCamera = GetComponent<Camera>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F5)) {
            BeginTakeScreenshot(Input.GetKey(KeyCode.LeftShift));
        }
    }

    #region Screenshot camera positioning
    private List<Vector3> screenshotCameraPositions = new List<Vector3>();

    public object Enviroment { get; private set; }

    private Vector3 GetAveragePosition() {
        float x = 0;
        float y = 0;

        foreach (Vector3 position in screenshotCameraPositions) {
            x += position.x;
            y += position.y;
        }

        return new Vector3(x / screenshotCameraPositions.Count, y / screenshotCameraPositions.Count, -20);
    }

    private float GetRequiredSize() {
        Vector3 desiredLocalPos = transform.InverseTransformPoint(GetAveragePosition());

        float size = 0;

        foreach (Vector3 position in screenshotCameraPositions) {
            Vector3 targetLocalPos = transform.InverseTransformPoint(position);
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / screenshotCamera.aspect);
        }

        
        if (cinematicScreenshot) {
            size += 0.5f;
        } else {
            size += 3.5f;
        }

        size = Mathf.Max(size, 3);

        return size;
    }

    private void PositionScreenshotCamera() {
        screenshotCameraPositions = new List<Vector3>();
        screenshotCameraPositions.Add(LevelLoader.Singletron.levelData.GetBounds().min);
        screenshotCameraPositions.Add(LevelLoader.Singletron.levelData.GetBounds().max);

        screenshotCamera.transform.position = GetAveragePosition();
        screenshotCamera.orthographicSize = GetRequiredSize();

        if (cinematicScreenshot) {
            screenshotCamera.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15, 15));
        } else {
            screenshotCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
    #endregion

    public IEnumerator TakeScreenshot() {
        if (cinematicScreenshot) {
            screenshotCamera.cullingMask = cinematicMask;
        } else {
            screenshotCamera.cullingMask = everythingMask;
        }

        PositionScreenshotCamera();

        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;

        RenderTexture rt = new RenderTexture(width, height, 24);
        screenshotCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshotCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();

        string levelsDir = Application.persistentDataPath + "/Level Screenshots/";

        if (!Directory.Exists(levelsDir)) Directory.CreateDirectory(levelsDir);

        File.WriteAllBytes(levelsDir + Path.GetFileNameWithoutExtension(LevelLoader.GetLevelDirectory()) + ".png", bytes);
    }

    private void BeginTakeScreenshot(bool cinematic) {
        cinematicScreenshot = cinematic;

        StartCoroutine(TakeScreenshot());
    }
}
