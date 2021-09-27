using UnityEngine;
using Steamworks;

public class DebugActivator : MonoBehaviour {
    public GameObject[] debugElements;

    //use start instead of awake so that steam has time to initialize
    private void Start() {
        if (SteamManager.Initialized && IsUserDebugger.GetIsUserDebugger()) {
            ActivateAll();
        } else {
            DeactivateAll();
        }
    }

    private void ActivateAll() {
        foreach(GameObject obj in debugElements) {
            obj.SetActive(true);
        }
    }

    private void DeactivateAll() {
        foreach (GameObject obj in debugElements) {
            obj.SetActive(false);
        }
    }
}
