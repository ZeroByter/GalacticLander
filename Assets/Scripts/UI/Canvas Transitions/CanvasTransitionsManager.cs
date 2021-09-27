using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasTransitionsManager : MonoBehaviour {
    public static CanvasTransitionsManager Singletron;

    public CanvasBlurTransition[] menus;
    public int defaultMenu;

    private void Awake() {
        Singletron = this;
    }

    private void Start() {
        for(int i = 0; i < menus.Length; i++) {
            CanvasBlurTransition menu = menus[i];

            if(i == defaultMenu) {
                menu.ForceOpen();
            } else {
                menu.ForceClose();
            }
        }
    }

    public void OpenMenu(int id) {
        for (int i = 0; i < menus.Length; i++) {
            CanvasBlurTransition menu = menus[i];

            if (i == id) {
                menu.OpenMenu();
            } else {
                menu.CloseMenu();
            }
        }
    }
}
