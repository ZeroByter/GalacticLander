using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using System.Diagnostics;

public class MainMenuManager : MonoBehaviour {
    public static MainMenuManager Singletron;

    public Transform playerShip;
    public TMP_Text averageScoreText;
    public Image blurImage;

    [DllImport("user32.dll")]
    private static extern void OpenFileDialog();

    private void Awake() {
        Singletron = this;

        SteamCustomUtils.SetAchievementNames();

        playerShip.position = new Vector3(UnityEngine.Random.Range(12, 30), 10, 0);
        playerShip.GetComponent<Rigidbody2D>().AddForce(Vector2.down * 15, ForceMode2D.Impulse);

        UpdateAverageScoreText();

        Physics2D.gravity = new Vector2(0, -0.8f);

        Shader.SetGlobalColor("_ReplaceColor", new Color(0.12f, 0.12f, 0.12f, 1));
        blurImage.material.SetFloat("_Size", 1.5f);

        //Update COMMUNITY_MEMBER achievement stats
        UGCQueryHandle_t handle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(), EUserUGCList.k_EUserUGCList_Published, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, EUserUGCListSortOrder.k_EUserUGCListSortOrder_CreationOrderAsc, new AppId_t(913600), new AppId_t(913600), 1);
        SteamUGC.SetReturnMetadata(handle, false);
        SteamUGC.SetReturnAdditionalPreviews(handle, false);
        SteamUGC.SetReturnChildren(handle, false);
        SteamUGC.SetAllowCachedResponse(handle, 4);
        SteamCallbacks.SteamUGCQueryCompleted_t.RegisterCallResult(SteamUGCQueryCompleted, SteamUGC.SendQueryUGCRequest(handle));
    }

    private void SteamUGCQueryCompleted(SteamUGCQueryCompleted_t callback, bool error)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
            throw new System.Exception("Got UGC query result with " + callback.m_eResult.ToString());
        }

        SteamCustomUtils.SetStat("WORKSHOP_LEVELS", (int)callback.m_unNumResultsReturned);

        SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
    }

    private void Update() {
        blurImage.material.SetFloat("_Size", Mathf.Lerp(blurImage.material.GetFloat("_Size"), 0, 0.04f));
    }

    public void UpdateAverageScoreText() {
        float bestAverageScore;
        float bestAverageTime;

        if (LevelProgressCounter.GetAverageScore(out bestAverageScore, out bestAverageTime)) {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<b><size=17>Your best average score: " + bestAverageScore + "</size></b>");
            sb.AppendLine("Your best average time: " + bestAverageTime);

            averageScoreText.text = sb.ToString();
        } else {
            averageScoreText.text = "";
        }
    }

    public void OpenLevelEditor() {
        SceneManager.LoadScene("Level Editor");
    }

    public void OpenFeedbackForums() {
        SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/app/913600/discussions/1/");
    }

    public void OpenGalacticLanderWebsite() {
        SteamFriends.ActivateGameOverlayToWebPage("https://galacticlander.zerobyter.net");
    }

    public void OpenSettings() {
        SettingsMenuManager.OpenMenu();
    }

    public void QuitApplication() {
        Application.Quit();
    }

    public void RefreshUI() {
        SceneManager.LoadScene("Main Menu");
    }
}
