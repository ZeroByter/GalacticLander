using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using System;

public class LobbyBrowserManager : MonoBehaviour {
    public static List<ulong> Lobbies = new List<ulong>();

    public static Action UpdateLobbiesNames;

    public CanvasTransitionsManager menusManager;
    public Color selectedFilterColor;
    [Header("The main menu button that leads to this browser menu")]
    public Button mainMenuButton;
    public GameObject mainMenuText;
    [Header("The lobby template")]
    public LobbyBrowserController lobbyTemplate;
    [Header("Filter buttons")]
    public LerpImageColor allLobbiesImage;
    public LerpImageColor friendsLobbiesImage;

    private enum CurrentLobbyFilter { All, FriendsOnly };
    private CurrentLobbyFilter currentLobbyFilter;

    private List<ulong> friendsLobbies = new List<ulong>();

    private float lastRefreshedLobbies;

    private void Awake() {
        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        mainMenuButton.interactable = false;

        if (SteamManager.Initialized && NetworkingManager.CurrentLobbyValid) {
            print("already in lobby, switching to lobby menu");
            menusManager.OpenMenu(5);

        }

        ClearLobbyList();

        mainMenuButton.interactable = SteamManager.Initialized;
        mainMenuText.SetActive(!SteamManager.Initialized);

        print("refreshing lobby list every one second");
    }

    private void OnEnable() {
        NetworkingManager.LobbyJoined += OnJoinedLobby;
        NetworkingManager.LobbyLeft += OnLobbyLeft;

        if (Lobbies == null) Lobbies = new List<ulong>();
        Lobbies.Clear(); //upon loading browser, clear all lobbies

        NetworkingManager.UpdateEventProgressData(); //update event progress data
    }

    private void OnDestroy() {
        StopAllCoroutines();
    }

    private void OnDisable() {
        NetworkingManager.LobbyJoined -= OnJoinedLobby;
        NetworkingManager.LobbyLeft -= OnLobbyLeft;
    }

    private void UpdateLobbyList() {
        if (!SteamManager.Initialized) return;

        if (currentLobbyFilter == CurrentLobbyFilter.FriendsOnly) {
            friendsLobbies.Clear();

            int friendsCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            for(int i = 0; i < friendsCount; i++) {
                ulong friendId = (ulong) SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);

                FriendGameInfo_t gameInfo;
                if(SteamFriends.GetFriendGamePlayed((CSteamID) friendId, out gameInfo)) {
                    if(gameInfo.m_gameID == new CGameID((ulong) SteamManager.AppId)) { //if this friend is playing the same game as us
                       if(gameInfo.m_steamIDLobby != (CSteamID) 0) { //if the friend is in any lobby
                            friendsLobbies.Add((ulong) gameInfo.m_steamIDLobby);
                        }
                    }
                }
            }
        }

        SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterFar);
        SteamMatchmaking.AddRequestLobbyListStringFilter("version", Constants.Version, ELobbyComparison.k_ELobbyComparisonEqual);
        SteamCallbacks.LobbyMatchList_t.RegisterCallResult(DisplayLobbiesList, SteamMatchmaking.RequestLobbyList());
    }

    private void Update() {
        if(Time.time > lastRefreshedLobbies + 1) {
            lastRefreshedLobbies = Time.time;

            UpdateLobbyList();
        }
    }

    private void DisplayLobbiesList(LobbyMatchList_t callback, bool success) {
        List<ulong> allLobbies = new List<ulong>();

        int numberOfLobbies = (int) callback.m_nLobbiesMatching;
        for(int i = 0; i < numberOfLobbies; i++) {
            ulong lobby = (ulong)SteamMatchmaking.GetLobbyByIndex(i);

            allLobbies.Add(lobby);
        }

        foreach (ulong lobby in allLobbies) {
            if (currentLobbyFilter == CurrentLobbyFilter.FriendsOnly && !friendsLobbies.Contains(lobby)) continue;

            if (Lobbies.Contains(lobby)) {
                if (UpdateLobbiesNames != null) UpdateLobbiesNames();
            } else {
                Lobbies.Add(lobby);
                AddTemplate(lobby);
            }
        }

        List<int> removeIndexes = new List<int>();

        foreach(ulong lobby in Lobbies) {
            if (!allLobbies.Contains(lobby)) removeIndexes.Add(Lobbies.IndexOf(lobby));
        }
        foreach(int indexToRemove in removeIndexes) {
            Lobbies.RemoveAt(indexToRemove);
        }
    }

    private void ClearLobbyList() {
        if (lobbyTemplate == null) return;

        foreach(Transform oldTemplate in lobbyTemplate.transform.parent) {
            if(oldTemplate.gameObject.activeSelf && oldTemplate.GetSiblingIndex() > 0) {
                Destroy(oldTemplate.gameObject);
            }
        }
    }

    private void AddTemplate(ulong lobbyId) {
        if (lobbyTemplate == null) return;

        LobbyBrowserController newTemplate = Instantiate(lobbyTemplate, lobbyTemplate.transform.parent);
        newTemplate.Setup(lobbyId);

        lobbyTemplate.transform.SetAsLastSibling();
    }

    private void OnLobbyLeft() {
        print("leaving lobby");
        menusManager.OpenMenu(4);
    }

    private void OnJoinedLobby(ulong lobbyId) {
        print("joined lobby - switching to lobby menu");
        menusManager.OpenMenu(5);
    }

    #region Public UI interaction methods
    private bool isCreatingLobby = false;

    public void CreateLobby() {
        if (isCreatingLobby) return;
        isCreatingLobby = true;

        if (NetworkingManager.CurrentLobbyValid) {
            print("trying to create a lobby while already in one?? leaving current lobby");
            SteamMatchmaking.LeaveLobby((CSteamID) NetworkingManager.CurrentLobby);
            return;
        } else {
            print("creating lobby");
            SteamCallbacks.LobbyCreated_t.RegisterCallResult(OnCreatedLobby, SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 2));

            //start/show loading prompt here - if we will ever want to do this
        }
    }

    private void OnCreatedLobby(LobbyCreated_t callback, bool error) {
        if (error || callback.m_eResult != EResult.k_EResultOK) {
            PromptsController.OpenErrorPrompt("Failed to create a lobby! Error code: " + callback.m_eResult.ToString());
            print("failed to create lobby - " + callback.m_eResult.ToString());
            return;
        }

        isCreatingLobby = false;

        NetworkingManager.CurrentLobby = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData((CSteamID) NetworkingManager.CurrentLobby, "name", SteamFriends.GetPersonaName() + "'s lobby");
        SteamMatchmaking.SetLobbyData((CSteamID) NetworkingManager.CurrentLobby, "version", Constants.Version);
        SteamMatchmaking.SetLobbyData((CSteamID) NetworkingManager.CurrentLobby, "nextObjectId", 0.ToString());
        SteamMatchmaking.SetLobbyData((CSteamID) NetworkingManager.CurrentLobby, "firstMovement", 0f.ToString());

        print("created lobby - switching to lobby menu");
        menusManager.OpenMenu(5);
        
        if (NetworkingManager.LobbyCreated != null) NetworkingManager.LobbyCreated(callback.m_ulSteamIDLobby);
    }

    public void ShowAllLobbies() {
        currentLobbyFilter = CurrentLobbyFilter.All;

        allLobbiesImage.target = selectedFilterColor;
        friendsLobbiesImage.target = Color.white;
    }

    public void ShowFriendsLobbies() {
        currentLobbyFilter = CurrentLobbyFilter.FriendsOnly;

        allLobbiesImage.target = Color.white;
        friendsLobbiesImage.target = selectedFilterColor;
    }
    #endregion
}
