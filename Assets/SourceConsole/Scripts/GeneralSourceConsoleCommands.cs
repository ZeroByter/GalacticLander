using UnityEngine.SceneManagement;
using Steamworks;
using SourceConsole;

public static class GeneralSourceConsoleCommands
{
    #region Goto Scenes
    [ConCommand]
    public static void goto_scene_mainmenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    [ConCommand]
    public static void goto_scene_leveleditor()
    {
        SceneManager.LoadScene("Level Editor");
    }
    #endregion

    #region Load levels
    [ConCommand]
    public static void load_level(string levelName)
    {
        if (levelName == "") return;

        var origin = LevelLoader.LevelOrigin.External;
        if (levelName.StartsWith("sp") || levelName.StartsWith("mp") || levelName.StartsWith("osp") || levelName.StartsWith("omp")) origin = LevelLoader.LevelOrigin.Game;

        LevelLoader.SetLevelDirectory(origin, levelName);
        SceneManager.LoadScene("Game Level");
    }

    [ConCommand]
    public static void load_eventlevel(string eventName, string levelIndex)
    {
        if (eventName == "" || levelIndex == "") return;

        LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.Game, $"{eventName}/{eventName}_{levelIndex}");
        SceneManager.LoadScene("Game Level");
    }
    #endregion

    #region Lobby stuff
    [ConCommand]
    public static void lobby_status()
    {
        if (!NetworkingManager.CurrentLobbyValid)
        {
            SourceConsole.SourceConsole.warn("Not currently in a lobby!");
        }
        else
        {
            CSteamID lobbyId = (CSteamID)NetworkingManager.CurrentLobby;

            SourceConsole.SourceConsole.print($"name: === {SteamMatchmaking.GetLobbyData(lobbyId, "name")} ({NetworkingManager.CurrentLobby}) ===");
            SourceConsole.SourceConsole.print($"version: {SteamMatchmaking.GetLobbyData(lobbyId, "version")}");
            SourceConsole.SourceConsole.print($"nextObject: {SteamMatchmaking.GetLobbyData(lobbyId, "name")}");
            SourceConsole.SourceConsole.print("");
            SourceConsole.SourceConsole.print($"all lobby data:");

            int count = SteamMatchmaking.GetLobbyDataCount(lobbyId);
            for (int i = 0; i < count; i++)
            {
                string key;
                string value;

                SteamMatchmaking.GetLobbyDataByIndex(lobbyId, i, out key, 10, out value, 24);

                SourceConsole.SourceConsole.print($"{i}/{key}: {value}");
            }
        }
    }

    [ConCommand]
    public static void lobby_getplayers()
    {
        if (!NetworkingManager.CurrentLobbyValid)
        {
            SourceConsole.SourceConsole.warn("Not currently in a lobby!");
        }
        else
        {
            SourceConsole.SourceConsole.print("Current players in lobby:");
            SourceConsole.SourceConsole.print("steamid steamname");
            foreach (var steamid in LobbyHelperFunctions.GetAllLobbyMembers())
            {
                SourceConsole.SourceConsole.print($"{steamid} {SteamFriends.GetFriendPersonaName((CSteamID)steamid)}");
            }
        }
    }
    #endregion
}
