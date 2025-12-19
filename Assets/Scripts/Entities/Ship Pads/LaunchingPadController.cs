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

    private int launchPadIndex = 0;

    private void Awake()
    {
        launchPadIndex = LaunchPads.Count;
        LaunchPads.Add(this);
    }

    private void Start()
    {
        bool isInLobby = SteamManager.Initialized && NetworkingManager.CurrentLobbyValid;

        var shipPosition = new Vector3(transform.position.x, transform.position.y) + transform.up * 0.56f;

        if (isInLobby)
        {
            bool areWeOwner = SteamMatchmaking.GetLobbyOwner((CSteamID)NetworkingManager.CurrentLobby) == SteamUser.GetSteamID();

            if (areWeOwner)
            {
                if (launchPadIndex - 1 + OwnerLaunchPadIndex == 0)
                {
                    NetworkingManager.InstantiateObject("Player Ships/Player Ship", shipPosition, transform.eulerAngles.z);
                }
                else
                {
                    NetworkingManager.SendPacketOtherOnly(new object[] { 12, shipPosition.x, shipPosition.y, transform.eulerAngles.z }, 1);
                }
            }
        }
        else
        {
            Instantiate(playerShip, shipPosition, transform.rotation);
        }
    }
}
