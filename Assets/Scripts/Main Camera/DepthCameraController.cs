using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DepthCameraController : MonoBehaviour {
    public float orthSizeOffset = -1;
    public Shader replacementShader;
    public bool refreshCamera;

    private Camera selfCamera;

    private void Awake() {
        selfCamera = GetComponent<Camera>();

        if (refreshCamera) StartCoroutine(RefreshCamera());
    }

    IEnumerator RefreshCamera() {
        selfCamera.enabled = false;
        yield return new WaitForSecondsRealtime(0.01f);
        selfCamera.enabled = true;
    }

    private void Update() {
        if (MainCameraController.Singletron == null) return;
        
        selfCamera.orthographicSize = MainCameraController.Singletron.selfCamera.orthographicSize + orthSizeOffset;
    }

    private void OnEnable() {
        if (replacementShader != null) selfCamera.SetReplacementShader(replacementShader, "");
    }

    private void OnDisable() {
        selfCamera.ResetReplacementShader();
    }
}
