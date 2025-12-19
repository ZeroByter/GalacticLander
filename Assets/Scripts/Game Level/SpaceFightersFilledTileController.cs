using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceFightersFilledTileController : MonoBehaviour {
    private bool triggered = false;

    private void Update() {
        Vector3 viewport = MainCameraController.Singletron.selfCamera.WorldToViewportPoint(transform.position);

        if(!triggered && viewport.x > 0 && viewport.x < 1 && viewport.y > 0 && viewport.y < 1) {
            triggered = true;

#if UNITY_EDITOR
            Debug.Log("can see space fighers");
#endif
            SteamCustomUtils.SetAchievement("RARE_SIGHT");
        }
    }
}
