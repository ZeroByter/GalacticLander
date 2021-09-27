using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Steamworks;
using TMPro;

public class OpenWorkshopLevelPage : MonoBehaviour, IPointerClickHandler {
    [HideInInspector]
    public ulong workshopId;

    private TMP_Text text;

    private void Awake() {
        text = GetComponent<TMP_Text>();
    }

    public void SetWorkshopId(ulong newId) {
        workshopId = newId;

        if(workshopId == 0) {
            text.color = Color.white;
            text.fontStyle = FontStyles.Normal;
        } else {
            text.color = new Color(0, 0.149019f, 1);
            text.fontStyle = FontStyles.Underline;
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(workshopId != 0) {
            SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/filedetails/?id=" + workshopId);
        }
    }
}
