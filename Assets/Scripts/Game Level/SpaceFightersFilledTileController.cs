using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceFightersFilledTileController : MonoBehaviour {
    private void Update() {
        Vector3 viewport = MainCameraController.Singletron.selfCamera.WorldToViewportPoint(transform.position);

        if(viewport.x > 0 && viewport.x < 1 && viewport.y > 0 && viewport.y < 1) {
            SteamCustomUtils.SetAchievement("RARE_SIGHT");
        }
    }
}
