using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UpdateUIShipSkinImage : MonoBehaviour {
    public static Action SkinChanged;

    private Image skinImage;

    private void Awake() {
        skinImage = GetComponent<Image>();
    }

    private void Start() {
        UpdateShipSkinImage();
    }

    public void OnEnable() {
        SkinChanged += OnSkinChanged;
    }

    private void OnDestroy() {
        SkinChanged += OnSkinChanged;
    }

    private void UpdateShipSkinImage() {
        if (skinImage == null) return;

        skinImage.sprite = ShipSkinsManager.Skins[ShipSkinsManager.SelectedSkin].GetSprite();
    }

    private void OnSkinChanged() {
        UpdateShipSkinImage();
    }
}
