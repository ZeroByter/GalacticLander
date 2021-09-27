using System.Collections;
using UnityEngine;

public class SelectShipSkinManager : MonoBehaviour {
    private static int AvailableSkins;

    public Transform template;
    public GameObject[] newShipSkinsText;

    private LerpCanvasGroup lerpGroup;

    private void Awake() {
        lerpGroup = GetComponent<LerpCanvasGroup>();

        template.gameObject.SetActive(false);
    }

    private void Start() {
        StartCoroutine(DisplayTemplates());
    }

    private void ClearTemplates() {
        foreach(Transform oldTemplate in template.parent) {
            if (oldTemplate.gameObject.activeSelf) Destroy(oldTemplate.gameObject);
        }
    }

    private void ShowNewSkinsText() {
        foreach(GameObject newText in newShipSkinsText) {
            newText.SetActive(true);
        }
    }

    private void HideNewSkinsText() {
        foreach (GameObject newText in newShipSkinsText) {
            newText.SetActive(false);
        }
    }

    private IEnumerator DisplayTemplates() {
        int oldAvailableSkins = AvailableSkins;
        AvailableSkins = 0;

        ClearTemplates();

        foreach(ShipSkin shipSkin in ShipSkinsManager.Skins) {
            bool createTemplate = false;

            if(shipSkin.requiredGameAchievement == "") {
                createTemplate = true;
            } else {
                CoroutineWithData cd = new CoroutineWithData(this, GameAchievementsManager.GetGameAchievement(shipSkin.requiredGameAchievement));
                yield return cd.coroutine;
                bool isAvailable = cd.result.ToString().StartsWith("true");

                if (isAvailable) {
                    createTemplate = true;
                }
            }

            if (shipSkin.developerOnly) {
                createTemplate = IsUserDebugger.GetIsUserDebugger();
            }

            if (createTemplate) {
                AvailableSkins++;

                SelectShipSkinController newTemplate = Instantiate(template, template.parent).GetComponent<SelectShipSkinController>();
                newTemplate.Setup(shipSkin);
            }
        }

        if(oldAvailableSkins != 0 && AvailableSkins > oldAvailableSkins) { //if we have more skins then before and if we didn't have zero skins before we show the 'new skins' text
            ShowNewSkinsText();
        } else {
            HideNewSkinsText();
        }
    }

    public void OpenMenu() {
        HideNewSkinsText();

        lerpGroup.target = 1;
    }

    public void CloseMenu() {
        lerpGroup.target = 0;
    }
}
