using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class LaunchingPadController : MonoBehaviour
{
    public static List<LaunchingPadController> LaunchPads = new List<LaunchingPadController>();
    public static int OwnerLaunchPadIndex = 0;

    [Header("The player ship we need to instantiate")]
    public Transform playerShip;

    private void Awake()
    {
        LaunchPads.Add(this);
    }

    private void Start()
    {
        bool isInLobby = SteamManager.Initialized && NetworkingManager.CurrentLobbyValid;

        if (isInLobby)
        {
            bool areWeOwner = SteamMatchmaking.GetLobbyOwner((CSteamID)NetworkingManager.CurrentLobby) == SteamUser.GetSteamID();

            if (areWeOwner)
            {
                if (LaunchPads[OwnerLaunchPadIndex] == this)
                {
                    NetworkingManager.InstantiateObject("Player Ships/Player Ship", new Vector2(transform.position.x, transform.position.y + 0.56f));
                }
                else
                {
                    NetworkingManager.SendPacketOtherOnly(new object[] { 12, transform.position.x, transform.position.y + 0.56f }, 1);
                }
            }
        }
        else
        {
            Instantiate(playerShip, new Vector2(transform.position.x, transform.position.y + 0.56f), Quaternion.identity);
        }
    }
}
