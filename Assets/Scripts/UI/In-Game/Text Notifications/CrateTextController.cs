using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CrateTextController : MonoBehaviour {
    public static bool isPlayerCarryingCrate;

    //private static CrateTextController Singletron;

    private TMP_Text text;
    private CanvasGroup canvasGroup;

    private void Awake() {
        //Singletron = this;

        text = GetComponent<TMP_Text>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
    }

    private string GetNormalCrateKey()
    {
        return KeysManager.GetKey(KeysManager.Player.Normal, KeysManager.Key.Crate).ToString();
    }

    private string GetSplitscreenCrateKey()
    {
        return KeysManager.GetKey(KeysManager.Player.Splitscreen, KeysManager.Key.Crate).ToString();
    }

    private void Update() {
        string textToDisplay = "";

        if (LevelLoader.IsPlayingSharedScreenCoop(false)) {
            foreach (PlayerShipController ship in PlayerShipController.Ships) {
                if (textToDisplay != "") break;

                if (ship.carriedCrate != null) {
                    textToDisplay = string.Format("press {0}/{1} to release", GetNormalCrateKey(), GetSplitscreenCrateKey());
                } else {
                    Vector2 playerPosition = ship.transform.position;
                    foreach (CrateController crate in CrateController.Crates) {
                        if (!crate.GetIsForcefullyAttachedToSensor() && Vector2.Distance(crate.transform.position, playerPosition) < Constants.DistanceNeededToPickupCrate) {
                            textToDisplay = string.Format("press {0}/{1} to pickup", GetNormalCrateKey(), GetSplitscreenCrateKey());
                        }
                    }
                }
            }
        } else {
            if (PlayerShipController.CarriedCrate != null) {
                textToDisplay = string.Format("press {0} to release", GetNormalCrateKey());
            } else {
                if (PlayerShipController.Singletron != null) {
                    Vector2 playerPosition = PlayerShipController.Singletron.transform.position;
                    foreach (CrateController crate in CrateController.Crates) {
                        if (!crate.GetIsForcefullyAttachedToSensor() && Vector2.Distance(crate.transform.position, playerPosition) < Constants.DistanceNeededToPickupCrate) {
                            textToDisplay = string.Format("press {0} to pickup", GetNormalCrateKey());
                        }
                    }
                }
            }
        }

        if(textToDisplay == "") {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, 0.5f);
        } else {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1, 0.5f);
            text.text = textToDisplay;
        }
    }
}
