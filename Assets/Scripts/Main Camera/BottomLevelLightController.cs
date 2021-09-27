using UnityEngine;
using UnityEngine.SceneManagement;

public class BottomLevelLightController : MonoBehaviour {
    void Update () {
        if (LevelLoader.Singletron != null && LevelLoader.Singletron.levelData != null) {
            Bounds levelBounds = LevelLoader.Singletron.levelData.GetBounds();
            transform.position = new Vector3(transform.position.x, levelBounds.center.y - levelBounds.extents.y + transform.localScale.y, 0);
        }
    }
}
