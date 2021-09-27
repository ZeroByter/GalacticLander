using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Steamworks;

public class SelectShipSkinController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public SelectShipSkinManager parentManager;
    public LerpCanvasGroup hoverBlur;
    public Image fillInSkin;
    
    private ShipSkin shipSkin;

    public void Setup(ShipSkin shipSkin) {
        this.shipSkin = shipSkin;

        fillInSkin.sprite = shipSkin.GetSprite();

        gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        hoverBlur.target = 0.7f;
    }

    public void OnPointerExit(PointerEventData eventData) {
        hoverBlur.target = 0f;
    }

    public void SelectSkin() {
        int skinIndex = ShipSkinsManager.Skins.IndexOf(shipSkin);

        ShipSkinsManager.SelectedSkin = skinIndex;
        
        if (UpdateUIShipSkinImage.SkinChanged != null) UpdateUIShipSkinImage.SkinChanged(); //only invoke if not valid, same as doing `if(UpdateUIShipSkinImage.skinChanged != null) UpdateUIShipSkinImage.skinChanged()`

        if (NetworkingManager.CurrentLobbyValid) {
            SteamMatchmaking.SetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, "selectedShipSkin", skinIndex.ToString());
        }

        parentManager.CloseMenu();
    }
}
