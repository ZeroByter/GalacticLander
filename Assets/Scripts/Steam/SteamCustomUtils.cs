using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;
using SourceConsole;

public class SteamCustomUtils {

    public static Dictionary<string, string> AchievementNames = new Dictionary<string, string>();

    public static void SetAchievementNames()
    {
        AchievementNames.Clear();

        AchievementNames.Add("HOVER_10", "Steady Hand");
        AchievementNames.Add("HOVER_20", "Engines Master");
        AchievementNames.Add("HOVER_30", "Balance Expert");
        AchievementNames.Add("ONE_FLIP", "Stuntsman");
        AchievementNames.Add("TWO_FLIPS", "Galactic Stuntsman");
        AchievementNames.Add("DAMAGE_SHIP_50", "Space Noob");
        AchievementNames.Add("DAMAGE_SHIP_100", "Repairmen Needed!");
        AchievementNames.Add("DAMAGE_SHIP_200", "Who Put A Wall There?");
        AchievementNames.Add("DAMAGE_SHIP_600", "Non-Functional");
        AchievementNames.Add("SP_PLAYED_6", "Lone Pilot");
        AchievementNames.Add("COOP_PLAYED_6", "Duo Pilots");
        AchievementNames.Add("ALL_SP_PLAYED", "Lone Explorer");
        AchievementNames.Add("ALL_COOP_PLAYED", "Duo Explorers");
        AchievementNames.Add("NO_DMG_3", "Sparkly Clean");
        AchievementNames.Add("NO_DMG_5", "Professional Pilot");
        AchievementNames.Add("NO_DMG_10", "Expert Navigator");
        AchievementNames.Add("WORKSHOP_CONTRIBUTER", "Community Member");
        AchievementNames.Add("NO_LEGS", "Legless Ship");
        AchievementNames.Add("NO_GRAVITY_3", "Galactic Expert");
        AchievementNames.Add("RARE_SIGHT", "Rare Archaeological Sight");
        AchievementNames.Add("DONT_DELETE_THAT", "Don't Delete That!");

        SourceConsole.SourceConsole.print($"Loaded {AchievementNames.Count} achievement names");
    }

    [ConVar]
    public static bool achievements_debugStats { get; set; }
    [ConVar]
    public static bool achievements_debugAchievements { get; set; }

    [ConCommand]
    public static void achievements_getnames()
    {
        foreach(var pair in AchievementNames)
        {
            SourceConsole.SourceConsole.print($"{pair.Key} = {pair.Value}");
        }
    }

    public static Texture2D GetSteamImageAsTexture2D(int iImage)
    {
        Texture2D ret = null;
        uint ImageWidth;
        uint ImageHeight;
        bool bIsValid = SteamUtils.GetImageSize(iImage, out ImageWidth, out ImageHeight);

        if (bIsValid)
        {
            byte[] Image = new byte[ImageWidth * ImageHeight * 4];

            bIsValid = SteamUtils.GetImageRGBA(iImage, Image, (int)(ImageWidth * ImageHeight * 4));
            if (bIsValid)
            {
                ret = new Texture2D((int)ImageWidth, (int)ImageHeight, TextureFormat.RGBA32, false, true);
                ret.LoadRawTextureData(Image);
                ret = FlipTexture(ret);
                ret.Apply();
            }
        }

        return ret;
    }

    public static bool GetUserAvatar(ulong steamID, System.Action<Texture2D> callback)
    {
        CSteamID cSteamID = new CSteamID(steamID);
        int userAvatar = SteamFriends.GetLargeFriendAvatar(cSteamID);
        uint imageWidth;
        uint imageHeight;

        bool success = SteamUtils.GetImageSize(userAvatar, out imageWidth, out imageHeight);

        if (success && imageWidth > 0 && imageHeight > 0)
        {
            byte[] data = new byte[imageWidth * imageHeight * 4];
            var returnTex = new Texture2D((int)imageWidth, (int)imageHeight, TextureFormat.RGBA32, false, false);

            success = SteamUtils.GetImageRGBA(userAvatar, data, (int)(imageWidth * imageHeight * 4));
            if (success)
            {
                returnTex.LoadRawTextureData(data);
                returnTex.Apply();

                //NOTE: texture loads upside down, so we flip it to normal...
                var result = FlipTexture(returnTex);

                callback(result);

                Texture2D.DestroyImmediate(returnTex);
            }
        }

        return success && imageWidth > 0 && imageHeight > 0;
    }

    public static Texture2D FlipTexture(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;

        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                flipped.SetPixel(i, yN - j - 1, original.GetPixel(i, j));
            }
        }

        flipped.Apply();

        return flipped;
    }

    public static void SetAchievement(string id)
    {
        if (!SteamManager.Initialized) return;
        if (SceneManager.GetActiveScene().name != "Game Level") return;

        bool alreadyAchieved;
        SteamUserStats.GetAchievement(id, out alreadyAchieved);
        if (alreadyAchieved) return;

        SteamUserStats.SetAchievement(id);
        SteamUserStats.StoreStats();

        if (achievements_debugAchievements) SourceConsole.SourceConsole.print($"Set achievement {id}");

        if (NetworkingManager.CurrentLobbyValid) { //if we are in a lobby
            NetworkingManager.SendPacket(new object[] { 10, id }, 1, EP2PSend.k_EP2PSendReliable); //send packet
        }
    }

    public static void SetLevelEditorAchievement(string id)
    {
        if (!SteamManager.Initialized) return;
        if (SceneManager.GetActiveScene().name != "Level Editor") return;

        bool alreadyAchieved;
        SteamUserStats.GetAchievement(id, out alreadyAchieved);
        if (alreadyAchieved) return;

        if (achievements_debugAchievements) SourceConsole.SourceConsole.print($"Set achievement {id}");

        SteamUserStats.SetAchievement(id);
        SteamUserStats.StoreStats();
    }

    [ConCommand("achievements_clear")]
    public static void ClearAchievement(string id)
    {
        if (!SteamManager.Initialized) return;

        SteamUserStats.ClearAchievement(id);
        SteamUserStats.StoreStats();
    }

    [ConCommand("achievements_clearall")]
    public static void ClearAllAchievements()
    {
        if (!SteamManager.Initialized) return;

        foreach(var pair in AchievementNames)
        {
            SteamUserStats.ClearAchievement(pair.Key);
        }
        SourceConsole.SourceConsole.print($"Cleared {AchievementNames.Count} achievements");

        SteamUserStats.StoreStats();
    }

    public static string GetAchievementName(string id)
    {
        if (AchievementNames.ContainsKey(id))
        {
            return AchievementNames[id];
        }
        else
        {
            return id;
        }
    }

    public static int GetStat(string id)
    {
        if (!SteamManager.Initialized) {
            Debug.Log(string.Format("Tried to get stat '{0}' but steam is not initialized, returning -1", id));
            return -1;
        }

        int data;

        SteamUserStats.GetStat(id, out data);
        //Debug.Log(string.Format("Stat '{0}' has value {1}", id, data));

        return data;
    }

    public static void SetStat(string id, int data)
    {
        if (!SteamManager.Initialized) return;

        SteamUserStats.SetStat(id, data);
        SteamUserStats.StoreStats();

        if(achievements_debugStats) SourceConsole.SourceConsole.print($"Set stat {id} to {data}");
    }

    public static void AddStat(string id, int amount = 1)
    {
        //if (SceneManager.GetActiveScene().name != "Game Level") return;

        SetStat(id, GetStat(id) + amount);
    }
}
