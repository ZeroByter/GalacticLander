using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SharedScreenCoopPromptController : MonoBehaviour {
    private Canvas canvas;
    private GraphicRaycaster raycaster;

    private void Awake() {
        canvas = GetComponent<Canvas>();
        raycaster = GetComponent<GraphicRaycaster>();
    }

    public void OpenPrompt() {
        canvas.enabled = true;
        raycaster.enabled = canvas.enabled;

        CursorController.AddUser("sharedScreenPrompt");
    }

    public void ClosePrompt() {
        canvas.enabled = false;
        raycaster.enabled = canvas.enabled;

        CursorController.RemoveUser("sharedScreenPrompt");
    }

    private void Update()
    {
        if (canvas.enabled && LastPressedEscape.LastPressedEscapeCooldownOver(0.1f))
        {
            if(Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Escape))
            {
                LastPressedEscape.SetPressedEscape();
                ClosePrompt();
            }
        }
    }

    private void Start() {
        bool openedPrompt = false;

        if (LevelLoader.IsPlayingSharedScreenCoop(true)) {
            openedPrompt = true;
            OpenPrompt();
        }

        if (!openedPrompt) {
            ClosePrompt();
        }
    }
}
