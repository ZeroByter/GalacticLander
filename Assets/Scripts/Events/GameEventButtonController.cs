using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEventButtonController : MonoBehaviour {
    public Image buttonRenderer;

    private void Start() {
        GameEvent currentEvent = GameEvents.GetCurrentEvent();

        if(currentEvent != null) {
            buttonRenderer.sprite = currentEvent.GetBannerResource();
        } else {
            buttonRenderer.gameObject.SetActive(false);
        }
    }
}
