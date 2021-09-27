using Steamworks;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDownloaderProgressPrompt : MonoBehaviour {
    public TMP_Text headerText;
    public LerpCanvasGroup lerpGroup;
    public Slider progressSlider;
    public TMP_Text progressText;
    
    private ulong bytesDownloaded = 0;
    private ulong bytesTotal = 0;

    private float lastCheckedWorkshop;

    private string levelName;
    private enum HeaderTextType { Downloading, PendingDownloading }
    private HeaderTextType headerTextType = HeaderTextType.PendingDownloading;

    private bool lastDownloadingLevel;

    private void ResetDownloadingProgress() {
        bytesDownloaded = 0;
        bytesTotal = 0;
    }

    private void FetchLevelName(PublishedFileId_t fileId) {
        levelName = "";

        UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[] { fileId }, 1);
        SteamUGC.SetReturnMetadata(handle, false);
        SteamUGC.SetReturnAdditionalPreviews(handle, false);
        SteamUGC.SetReturnChildren(handle, false);
        SteamUGC.SetAllowCachedResponse(handle, 4);
        SteamAPICall_t apiCall = SteamUGC.SendQueryUGCRequest(handle);
        SteamCallbacks.SteamUGCQueryCompleted_t.RegisterCallResult(SteamUGCQueryCompleted, apiCall);
    }

    private void SteamUGCQueryCompleted(SteamUGCQueryCompleted_t callback, bool error) {
        if (callback.m_eResult != EResult.k_EResultOK) {
            SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
            throw new System.Exception("Got UGC query result with " + callback.m_eResult.ToString());
        }

        for (int i = 0; i < callback.m_unNumResultsReturned; i++) {
            SteamUGCDetails_t details;
            if (SteamUGC.GetQueryUGCResult(callback.m_handle, (uint)i, out details)) {
                if (details.m_eResult == EResult.k_EResultOK) {
                    print("got level name, it is " + details.m_rgchTitle);
                    levelName = details.m_rgchTitle;
                }
            }
        }

        SteamUGC.ReleaseQueryUGCRequest(callback.m_handle);
    }

    private void Update() {
        if(Time.time > lastCheckedWorkshop + 0.1f) {
            lastCheckedWorkshop = Time.time;

            PublishedFileId_t[] subscribedItems = new PublishedFileId_t[SteamUGC.GetNumSubscribedItems()];
            SteamUGC.GetSubscribedItems(subscribedItems, (uint)subscribedItems.Length);

            bool downloadingLevel = false;
            ulong downloadingLevelId = 0;

            foreach(PublishedFileId_t file in subscribedItems) {
                EItemState itemState = (EItemState)SteamUGC.GetItemState(file);

                if(itemState.HasFlag(EItemState.k_EItemStateNeedsUpdate)) {
                    if (SteamUGC.GetItemDownloadInfo(file, out bytesDownloaded, out bytesTotal)) {
                        downloadingLevelId = file.m_PublishedFileId;
                        downloadingLevel = true;
                        headerTextType = HeaderTextType.Downloading;
                        break;
                    } else {
                        if (itemState.HasFlag(EItemState.k_EItemStateDownloadPending)) {
                            headerTextType = HeaderTextType.PendingDownloading;
                            downloadingLevelId = file.m_PublishedFileId;
                            downloadingLevel = true;
                        } else {
                            ResetDownloadingProgress();
                        }
                    }
                } else {
                    ResetDownloadingProgress();
                }
            }

            if (!downloadingLevel) {
                ResetDownloadingProgress();
            }

            if(lastDownloadingLevel != downloadingLevel) { //if the downloading level status changed (aka we just started/stopped downloading a level
                if (downloadingLevel && downloadingLevelId != 0) { //if we are downloading a level
                    FetchLevelName(new PublishedFileId_t(downloadingLevelId));
                }
            }
            lastDownloadingLevel = downloadingLevel;
        }

        if(headerTextType == HeaderTextType.Downloading) {
            headerText.text = string.Format("Downloading level '{0}'", levelName);
        } else {
            headerText.text = string.Format("Preparing to download level '{0}'", levelName);
        }

        lerpGroup.target = bytesTotal != 0 ? 1 : 0;
        if (bytesTotal != 0) {
            progressSlider.value = bytesDownloaded / bytesTotal;
            progressText.text = (bytesDownloaded / bytesTotal * 100) + "%";

            if (NetworkingManager.CurrentLobbyValid) {
                SteamMatchmaking.SetLobbyMemberData(SteamUser.GetSteamID(), "workshopleveldownloadpercent", (bytesDownloaded / bytesTotal * 100).ToString());
            }
        } else {
            ResetDownloadingProgress();

            if (NetworkingManager.CurrentLobbyValid) {
                SteamMatchmaking.SetLobbyMemberData(SteamUser.GetSteamID(), "workshopleveldownloadpercent", 0.ToString());
            }
        }
    }
}
