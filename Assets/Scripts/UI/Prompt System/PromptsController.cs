using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PromptsController : MonoBehaviour {
    private static PromptsController Singletron;

    [Header("The error prompt")]
    public LerpCanvasGroup errorCanvasLerp;
    public TMP_Text errorHeaderText;
    
    private GraphicRaycaster raycaster;

    //private enum OpenPrompt { None, Error }
    //private OpenPrompt currentOpenPrompt = OpenPrompt.None;

    private void Awake() {
        if(Singletron != null) {
            Destroy(gameObject);
            return;
        }

        Singletron = this;
        DontDestroyOnLoad(gameObject);

        raycaster = GetComponent<GraphicRaycaster>();
        raycaster.enabled = false;

        errorCanvasLerp.ForceAlpha(0);
    }

    private void Update() {
        if(errorCanvasLerp.target == 0 && errorCanvasLerp.currentAlpha < 0.1f) {
            errorCanvasLerp.ForceAlpha(0);
        }
    }

    public static void CloseAllPrompts() {
        if (Singletron == null) return;

        //Singletron.currentOpenPrompt = OpenPrompt.None;

        Singletron.errorCanvasLerp.target = 0;
        Singletron.raycaster.enabled = false;
    }

    public static void OpenErrorPrompt(string header) {
        if (Singletron == null) return;

        //Singletron.currentOpenPrompt = OpenPrompt.Error;

        Singletron.errorHeaderText.text = header;
        Singletron.errorCanvasLerp.target = 1;
        Singletron.raycaster.enabled = true;
    }

    public void _CloseAllPrompts() {
        CloseAllPrompts();
    }
}
