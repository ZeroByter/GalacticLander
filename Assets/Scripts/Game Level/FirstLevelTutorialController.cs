using UnityEngine;
using TMPro;

public class FirstLevelTutorialController : MonoBehaviour {
    public TMP_Text topText;
    public TMP_Text middleText;
    public TMP_Text bottomText;
    public TMP_Text middleOfLevelText;

    private bool showText;
    private bool playerPassedHalfOfLevel = false;

    private LevelData levelData;

    private void Awake() {
        topText.color = new Color(1, 1, 1, 0);
        middleText.color = new Color(1, 1, 1, 0);
        bottomText.color = new Color(1, 1, 1, 0);
        middleOfLevelText.color = new Color(1, 1, 1, 0);
    }

    private void Start() {
        if(LevelLoader.GetLevelDirectory() == "sp1") {
            showText = true;

            //find the launcher pad
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Entity")) {
                if(go.name == "Launch Pad(Clone)") {
                    transform.position = (Vector2) go.transform.position + new Vector2(0, 1.2f);
                    break;
                }
            }

            levelData = LevelLoader.Singletron.levelData;

            middleOfLevelText.transform.position = Vector2.Lerp(levelData.GetLaunchPad().GetPosition(), levelData.GetLandPad().GetPosition(), 0.5f) + new Vector2(0, 2);

        }
    }

    private void Update() {
        if (showText) {
            topText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, Time.timeSinceLevelLoad / 3));
            middleText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, Time.timeSinceLevelLoad / 3 - 1.5f));
            bottomText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, Time.timeSinceLevelLoad / 2 - 3));

            if(levelData != null) {
                Vector2 launchPosition = levelData.GetLaunchPad().GetPosition();
                Vector2 landPosition = levelData.GetLandPad().GetPosition();
                float distanceFromLaunchToLand = Vector2.Distance(launchPosition, Vector2.Lerp(launchPosition, landPosition, 0.5f));
                
                if(Vector2.Distance(PlayerShipController.Singletron.transform.position, launchPosition) > distanceFromLaunchToLand - 1) {
                    playerPassedHalfOfLevel = true;
                }
            }

            if (playerPassedHalfOfLevel) {
                middleOfLevelText.color = Color.Lerp(middleOfLevelText.color, new Color(1, 1, 1, 1), 0.004f);
            }
        }
    }
}
