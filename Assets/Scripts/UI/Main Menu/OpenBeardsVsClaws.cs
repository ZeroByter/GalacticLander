using UnityEngine;
using Steamworks;

public class OpenBeardsVsClaws : MonoBehaviour
{
    public void OnClick()
    {
        SteamFriends.ActivateGameOverlayToWebPage("https://store.steampowered.com/app/4329940/Beards_vs_Claws/");
    }
}
