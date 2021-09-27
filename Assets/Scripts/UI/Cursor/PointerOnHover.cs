using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class PointerOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    private bool isMouseOver;

    private Button button;
    private TMP_InputField inputField;

    private void Awake() {
        button = GetComponent<Button>();
        inputField = GetComponent<TMP_InputField>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (button != null && !button.interactable) return;
        if (inputField != null && !inputField.interactable) return;

        isMouseOver = true;
        CursorController.AddUser("HoverPointer", CursorUser.Type.Pointer);
    }

    public void OnPointerExit(PointerEventData eventData) {
        isMouseOver = false;
        CursorController.RemoveUser("HoverPointer");
    }

    private void OnDisable() {
        if (isMouseOver) {
            CursorController.RemoveUser("HoverPointer");
        }
    }

    private IEnumerator ButtonClickedCheck()
    {
        yield return new WaitForSecondsRealtime(0.01f);

        if (!button.interactable) CursorController.RemoveUser("HoverPointer");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button == null) return;
        if (!button.interactable) return;

        StartCoroutine(ButtonClickedCheck());
    }
}
