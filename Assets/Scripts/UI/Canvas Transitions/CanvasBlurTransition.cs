using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasBlurTransition : MonoBehaviour {
    [Header("Variables")]
    public float openingLerpSpeed = 0.5f;
    public float closingLerpSpeed = 0.5f;
    public float openBlurSize = 1.5f;
    public string cursorUser = "";
    public bool closeOnEscapePressed = false;
    [Header("Canvas and raycaster")]
    public Canvas canvas;
    public GraphicRaycaster raycaster;
    public CanvasGroup canvasGroup;

    public bool isOpen;

    private Image blurImage;

    private void Awake() {
        blurImage = GetComponent<Image>();

        canvas = transform.parent.GetComponent<Canvas>();
        raycaster = transform.parent.GetComponent<GraphicRaycaster>();
        canvasGroup = transform.parent.GetComponent<CanvasGroup>();

        if(blurImage != null) blurImage.material.SetFloat("_Size", isOpen ? openBlurSize : 0);

        Canvas.ForceUpdateCanvases();
    }

    private void OnDisable() {
        if (cursorUser != "") CursorController.RemoveUser(cursorUser);
    }

    private void Update() {
        float currentBlur;

        if(blurImage != null) { //if blur image exists
            currentBlur = blurImage.material.GetFloat("_Size");

            if (isOpen) {
                blurImage.material.SetFloat("_Size", Mathf.Lerp(currentBlur, openBlurSize, openingLerpSpeed));
            } else {
                blurImage.material.SetFloat("_Size", Mathf.Lerp(currentBlur, 0, closingLerpSpeed));
            }
        } else { //if blur image doens't exist
            if(canvasGroup != null) { //if canvas group exists
                if (isOpen) {
                    currentBlur = Mathf.Lerp(canvasGroup.alpha, openBlurSize, openingLerpSpeed);
                } else {
                    currentBlur = Mathf.Lerp(canvasGroup.alpha, 0, closingLerpSpeed);
                }
            } else { //if it doesn't
                currentBlur = isOpen ? openBlurSize : 0;
            }
        }

        if (canvas != null) canvas.enabled = currentBlur > openBlurSize * 0.15;
        if (raycaster != null) raycaster.enabled = currentBlur > openBlurSize * 0.15f;
        if (canvasGroup != null) canvasGroup.alpha = currentBlur / openBlurSize;

        if(closeOnEscapePressed && isOpen && Input.GetKey(KeyCode.Escape) && LastPressedEscape.LastPressedEscapeCooldownOver(0.1f)) {
            LastPressedEscape.SetPressedEscape();
            CloseMenu();
        }
    }

    public void ForceOpen() {
        if (cursorUser != "") CursorController.AddUser(cursorUser);
        isOpen = true;

        if (blurImage != null) {
            blurImage.material.SetFloat("_Size", openBlurSize);
        } else {
            if (canvasGroup != null) canvasGroup.alpha = openBlurSize;
        }
    }

    public void ForceClose() {
        if (cursorUser != "") CursorController.RemoveUser(cursorUser);
        isOpen = false;

        if (blurImage != null) {
            blurImage.material.SetFloat("_Size", 0);
        } else {
            if (canvasGroup != null) canvasGroup.alpha = 0;
        }
    }

    public void OpenMenu() {
        if (cursorUser != "") CursorController.AddUser(cursorUser);
        isOpen = true;
    }

    public void CloseMenu() {
        if (cursorUser != "") CursorController.RemoveUser(cursorUser);
        isOpen = false;
    }

    public void ToggleMenu() {
        if (isOpen) {
            CloseMenu();
        } else {
            OpenMenu();
        }
    }
}
