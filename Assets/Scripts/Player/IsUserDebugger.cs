using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IsUserDebugger {
    public static ulong[] Debuggers = new ulong[] { 76561198040596004, 76561198204233851 };

    public static bool GetIsUserDebugger() {
        if (!SteamManager.Initialized) return false;

        return Debuggers.Contains(SteamUser.GetSteamID().m_SteamID);
    }
}
