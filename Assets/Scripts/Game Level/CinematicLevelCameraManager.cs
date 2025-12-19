using UnityEngine;
using SourceConsole;
using System.Collections;

public class CinematicLevelCameraManager : MonoBehaviour {
    private static CinematicLevelCameraManager Singleton;

    [ConCommand]
    public static void CinematicLevelCamera_PhotographGameLevels(int amount)
    {
        if (Singleton == null) return;

        Singleton.StartCoroutine(Singleton._PhotographGameLevels(amount));
    }

    private void Awake() {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator _PhotographGameLevels(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SourceConsole.SourceConsole.ExecuteString($"load_level mp{i + 1}");

            yield return new WaitForSeconds(0.5f);

            SourceConsole.SourceConsole.ExecuteString($"cinematiclevelcamera_takescreenshot true");

            yield return new WaitForSeconds(0.5f);
        }

        SourceConsole.SourceConsole.ExecuteString($"goto_scene_mainmenu");
    }
}
