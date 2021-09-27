using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonToggle : MonoBehaviour, IPointerClickHandler {
    public bool isToggled = false;
    public Color toggledColor = Color.white;
    public Color inactiveColor = new Color(0, 0, 0, 0);
    [Header("Should pressing the button toggle the toggled state")]
    public bool toggleOnPress = true;

    private Button button;

    private void Awake() {
        button = GetComponent<Button>();

        SetToggle(isToggled);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if(toggleOnPress) SetToggle(!isToggled);
    }

    public void SetToggle(bool toggled) {
        isToggled = toggled;

        ColorBlock colors = button.colors;
        if (isToggled) {
            colors.normalColor = toggledColor;
        } else {
            colors.normalColor = inactiveColor;
        }
        button.colors = colors;
    }
}
