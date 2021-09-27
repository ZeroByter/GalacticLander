using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class LobbyMenuController : MonoBehaviour {
    [Header("The menu manager")]
    public CanvasTransitionsManager menusManager;

    [Header("Chat stuff")]
    public ChatEntryController chatTemplate;
    public TMP_InputField chatInputField;

    [Header("Lobby name text")]
    public TMP_Text lobbyName;

    [Header("Player #1 UI")]
    public Image player1ShipSkin;
    public TMP_Text player1Name;
    public GameObject player1SteamFriendIcon;
    public TMP_Text player1LevelVoted;
    public OpenWorkshopLevelPage player1OpenWorkshop;

    [Header("Player #2 UI")]
    public Image player2ShipSkin;
    public LerpCanvasGroup player2CanvasGroup;
    public LerpCanvasGroup kickPlayer2CanvasGroup;
    public GameObject player2SteamFriendIcon;
    public TMP_Text player2Name;
    public TMP_Text player2LevelVoted;
    public OpenWorkshopLevelPage player2OpenWorkshop;

    [Header("Other lobby buttons")]
    public Button startGameButton;

    private bool isSecondPlayerConnected;
    private bool showSecondPlayerKickButton;
    private bool isSecondPlayerSteamFriend;

    private bool shouldShowKickPlayerGroup;

    private bool lastIsLobbyOwner;

    private void Awake() {
        startGameButton.interactable = false;

        lastIsLobbyOwner = false;

        chatTemplate.gameObject.SetActive(false);
    }

    private void OnEnable() {
        if (SteamManager.Initialized) {
            SteamCallbacks.LobbyChatMsg_t.RegisterCallback(LobbyChatMessageReceived);
            SteamCallbacks.LobbyChatUpdate_t.RegisterCallback(LobbyStateChanged);
            SteamCallbacks.LobbyDataUpdate_t.RegisterCallback(LobbyDataUpdated);
        }

        NetworkingManager.LobbyJoined += JoinedLobby;
        NetworkingManager.LobbyCreated += LobbyCreated;
    }

    private void OnDisable() {
        SteamCallbacks.LobbyChatMsg_t.UnregisterCallback(LobbyChatMessageReceived);
        SteamCallbacks.LobbyChatUpdate_t.UnregisterCallback(LobbyStateChanged);
        SteamCallbacks.LobbyDataUpdate_t.UnregisterCallback(LobbyDataUpdated);

        NetworkingManager.LobbyJoined -= JoinedLobby;
        NetworkingManager.LobbyCreated -= LobbyCreated;
    }

    private void Start() {
        if(SteamManager.Initialized && NetworkingManager.CurrentLobbyValid) {
            UpdateUI();

            SteamMatchmaking.SetLobbyMemberData((CSteamID) NetworkingManager.CurrentLobby, "isInGame", "false");

            SteamMatchmaking.SetLobbyMemberData(SteamUser.GetSteamID(), "currentEventProgression", GameEvents.CurrentEventProgression.ToString());
            SteamMatchmaking.SetLobbyMemberData(SteamUser.GetSteamID(), "currentEventCoopProgression", GameEvents.CurrentEventCoopProgression.ToString());
        }

        NetworkingManager.ResetNetwork();
    }

    private void Update() {
        kickPlayer2CanvasGroup.target = shouldShowKickPlayerGroup ? 1 : 0;
    }

    private void LobbyStateChanged(LobbyChatUpdate_t callback) {
        UpdateUI();
    }

    private bool clearedOldChat = false;

    private void LobbyCreated(ulong lobbyId) {
        if (lobbyId != 0) return;

        UpdateUI();

        lastIsLobbyOwner = false;
        clearedOldChat = false;

        ClearChatList();
    }

    private void JoinedLobby(ulong lobbyId) {
        SteamMatchmaking.SetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, "isInGame", "false");
        UpdateUI(); //since we have just joined the lobby, we must update all the ui stuff (such as player names, current map/votes, etc)

        lastIsLobbyOwner = false;
        clearedOldChat = false;

        ClearChatList();
    }

    public static ulong GetNonOwnerPlayer() {
        ulong ownerId = (ulong) SteamMatchmaking.GetLobbyOwner((CSteamID)NetworkingManager.CurrentLobby);

        int numOfMembers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)NetworkingManager.CurrentLobby);
        for(int i = 0; i < numOfMembers; i++) {
            ulong id = (ulong) SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)NetworkingManager.CurrentLobby, i);

            if (id != ownerId) return id;
        }

        return 0;
    }

    public static ulong GetOtherPlayer() {
        ulong selfId = SteamUser.GetSteamID().m_SteamID;

        int numOfMembers = SteamMatchmaking.GetNumLobbyMembers((CSteamID)NetworkingManager.CurrentLobby);
        for (int i = 0; i < numOfMembers; i++) {
            ulong id = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)NetworkingManager.CurrentLobby, i);

            if (id != selfId) return id;
        }

        return 0;
    }

    private bool IsPlayerAFriend(ulong searchId) {
        int numOfFriends = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagNone);

        for(int i = 0; i < numOfFriends; i++) {
            ulong friendId = (ulong) SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagNone);

            if (friendId == searchId) return true;
        }

        return false;
    }

    private bool AreWeLobbyOwner() {
        if (!NetworkingManager.CurrentLobbyValid) return false;

        return SteamMatchmaking.GetLobbyOwner((CSteamID)NetworkingManager.CurrentLobby) == SteamUser.GetSteamID();
    }

    private int GetPlayerShipSprite(ulong player) {
        int shipIndex = 0;

        int.TryParse(SteamMatchmaking.GetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, (CSteamID)player, "selectedShipSkin"), out shipIndex);

        return shipIndex;
        //return ship sprite, use int.TryParse, if its invalid return 0
        //TODO: fix replacement sprite wrong position!
    }

    public void UpdateUI() {
        CSteamID currentLobby = (CSteamID)NetworkingManager.CurrentLobby;

        if (clearedOldChat) {
            if (lastIsLobbyOwner != AreWeLobbyOwner() && AreWeLobbyOwner()) { //if our lobby owner status changed over this update ui method
                AddChatTemplate("As the lobby's owner, you can change the lobby's name by typing '/setname {name}'");
            }
            lastIsLobbyOwner = AreWeLobbyOwner();
        }

        //we update the lobby name text
        lobbyName.text = SteamMatchmaking.GetLobbyData(currentLobby, "name");

        //first we take care of the lobby owner, always at the left most ship UI
        ulong lobbyOwner = (ulong)SteamMatchmaking.GetLobbyOwner(currentLobby);
        player1ShipSkin.sprite = ShipSkinsManager.Skins[GetPlayerShipSprite(lobbyOwner)].GetSprite();
        string player1DownloadingPercent = SteamMatchmaking.GetLobbyMemberData(currentLobby, (CSteamID)lobbyOwner, "workshopleveldownloadpercent");
        if(player1DownloadingPercent == "0" || player1DownloadingPercent == "") {
            player1Name.text = SteamFriends.GetFriendPersonaName((CSteamID)lobbyOwner);
        } else {
            player1Name.text = SteamFriends.GetFriendPersonaName((CSteamID)lobbyOwner) + " - " + player1DownloadingPercent + "%";
        }
        player1SteamFriendIcon.SetActive(IsPlayerAFriend(lobbyOwner));
        bool player1GravityMode = SteamMatchmaking.GetLobbyMemberData(currentLobby, (CSteamID)lobbyOwner, "voteGravity") == "True";
        string player1Level = SteamMatchmaking.GetLobbyMemberData(currentLobby, (CSteamID)lobbyOwner, "levelvoted");
        string player1LevelWorkshopName = SteamMatchmaking.GetLobbyMemberData(currentLobby, (CSteamID)lobbyOwner, "levelvotedworkshopname"); ;
        if (!string.IsNullOrEmpty(player1LevelWorkshopName)) { //if player 1 has voted for a workshop level
            player1OpenWorkshop.SetWorkshopId(ulong.Parse(player1Level)); //we set player 1's openworkshop component accordingly
        } else {
            player1OpenWorkshop.SetWorkshopId(0); //if player 1 didn't vote for a workshop level, disable
        }
        if(player1Level == "event") {
            player1LevelVoted.text = GameEvents.GetCurrentEvent().displayName + " Event";
        } else {
            player1LevelVoted.text = GetLevelDisplayName(player1Level, player1LevelWorkshopName);
        }

        //now we take care of the second player, if any
        if (SteamMatchmaking.GetNumLobbyMembers(currentLobby) == 2) { //there is a second player!
            ulong otherPlayer = GetNonOwnerPlayer();

            player2ShipSkin.sprite = ShipSkinsManager.Skins[GetPlayerShipSprite(otherPlayer)].GetSprite();
            player2ShipSkin.enabled = true;
            string player2DownloadingPercent = SteamMatchmaking.GetLobbyMemberData(currentLobby, (CSteamID)otherPlayer, "workshopleveldownloadpercent");
            if (player2DownloadingPercent == "0" || player2DownloadingPercent == "") {
                player2Name.text = SteamFriends.GetFriendPersonaName((CSteamID)otherPlayer);
            } else {
                player2Name.text = SteamFriends.GetFriendPersonaName((CSteamID)otherPlayer) + " - " + player2DownloadingPercent + "%";
            }
            player2CanvasGroup.target = 1f;
            player2SteamFriendIcon.SetActive(IsPlayerAFriend(otherPlayer));
            bool player2GravityMode = SteamMatchmaking.GetLobbyMemberData(currentLobby, (CSteamID)otherPlayer, "voteGravity") == "True";
            string player2Level = SteamMatchmaking.GetLobbyMemberData(currentLobby, (CSteamID)otherPlayer, "levelvoted");
            string player2LevelWorkshopName = SteamMatchmaking.GetLobbyMemberData(currentLobby, (CSteamID)otherPlayer, "levelvotedworkshopname");
            if (!string.IsNullOrEmpty(player2LevelWorkshopName)) { //if player 2 has voted for a workshop level
                player2OpenWorkshop.SetWorkshopId(ulong.Parse(player2Level)); //we set player 2's openworkshop component accordingly
            } else {
                player2OpenWorkshop.SetWorkshopId(0); //if player 2 didn't vote for a workshop level, disable
            }
            if (player2Level == "event") {
                player2LevelVoted.text = GameEvents.GetCurrentEvent().displayName + " Event";
            } else {
                player2LevelVoted.text = GetLevelDisplayName(player2Level, player2LevelWorkshopName);
            }

            SteamFriends.SetPlayedWith((CSteamID)otherPlayer);
            
            if(SteamUser.GetSteamID() == (CSteamID) lobbyOwner && player1Level != "" && player2Level != "" && player1Level == player2Level && player1GravityMode == player2GravityMode) {
                startGameButton.interactable = true; //can only start the game once both players agree to the same level and we are the owner of the lobby
            } else {
                startGameButton.interactable = false;
            }
        } else { //there is no second player, we are all alone in this lobby (as the lobby owner)
            player2Name.text = "";
            player2CanvasGroup.target = 0.25f;
            player2SteamFriendIcon.SetActive(false);
            player2LevelVoted.text = "";
            player2OpenWorkshop.SetWorkshopId(0);
            startGameButton.interactable = false;
            player2ShipSkin.sprite = ShipSkinsManager.Skins[0].GetSprite();
            player2ShipSkin.enabled = false;
        }
    }

    private string GetLevelDisplayName(string levelName, string levelWorkshopName) {
        if (string.IsNullOrEmpty(levelName)) return "";

        if(levelName.StartsWith("sp") || levelName.StartsWith("mp")) {
            return "Level #" + levelName.Replace("sp", "").Replace("mp", "");
        } else {
            return levelWorkshopName;
        }
    }

    private void LobbyDataUpdated(LobbyDataUpdate_t callback) {
        if (callback.m_ulSteamIDLobby != NetworkingManager.CurrentLobby) return;

        UpdateUI();
    }

    public void StartGame() {
        NetworkingManager.SendPacket(new object[] { 0 }, 0, EP2PSend.k_EP2PSendReliable);
    }

    public void LeaveLobby() {
        print("Leaving lobby right now");
        menusManager.OpenMenu(4);
        SteamMatchmaking.LeaveLobby((CSteamID) NetworkingManager.CurrentLobby);
        NetworkingManager.CurrentLobby = 0;
    }

    public void ShowKickPlayerGroup() {
        if (SteamUser.GetSteamID() != SteamMatchmaking.GetLobbyOwner((CSteamID)NetworkingManager.CurrentLobby) || SteamMatchmaking.GetNumLobbyMembers((CSteamID)NetworkingManager.CurrentLobby) <= 1) {
            HideKickPlayerGroup();
            return;
        }

        shouldShowKickPlayerGroup = true;
    }

    public void HideKickPlayerGroup() {
        shouldShowKickPlayerGroup = false;
    }

    public void KickPlayer() {
        NetworkingManager.SendPacketOtherOnly(new object[] { 1 }, 0, EP2PSend.k_EP2PSendReliable);
        //SteamNetworking.SendP2PPacket((CSteamID)GetNonOwnerPlayer(), new byte[] { 1 }, 1, EP2PSend.k_EP2PSendReliable, 0);
    }

    #region Chat
    private void LobbyChatMessageReceived(LobbyChatMsg_t callback) {
        if (callback.m_ulSteamIDLobby != NetworkingManager.CurrentLobby) return;

        byte[] bytes = new byte[512];
        CSteamID steamId;
        EChatEntryType type;

        SteamMatchmaking.GetLobbyChatEntry((CSteamID)callback.m_ulSteamIDLobby, (int)callback.m_iChatID, out steamId, bytes, 512, out type);

        //deserialize the bytes into a string
        using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length)) {
            BinaryFormatter bf = new BinaryFormatter();
            object dataObject = bf.Deserialize(ms);
            AddChatTemplate((string)dataObject);
        }
    }

    public void ClearChatList() {
        foreach(Transform template in chatTemplate.transform.parent) {
            if (template.gameObject.activeSelf) Destroy(template.gameObject);
        }

        clearedOldChat = true;
    }

    public void AddChatTemplate(string text) {
        ChatEntryController newTemplate = Instantiate(chatTemplate, chatTemplate.transform.parent);
        newTemplate.Setup(text);
    }

    public void SendChatText() {
        if (string.IsNullOrEmpty(chatInputField.text)) return;

        if (chatInputField.text.StartsWith("/setname")) {
            string newLobbyName = chatInputField.text.Replace("/setname ", "");
            if(newLobbyName.Length > 36) {
                newLobbyName = newLobbyName.Substring(0, 36);
            }
            
            SteamMatchmaking.SetLobbyData((CSteamID)NetworkingManager.CurrentLobby, "name", newLobbyName);

            UpdateUI();
        } else {
            string text = "<b>" + SteamFriends.GetPersonaName() + "</b>: " + chatInputField.text;
            byte[] rawText;

            //serialize the text and convert into bytes
            using (MemoryStream ms = new MemoryStream()) {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, text);
                rawText = ms.ToArray();
            }

            SteamMatchmaking.SendLobbyChatMsg((CSteamID)NetworkingManager.CurrentLobby, rawText, rawText.Length + 1);
        }

        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }
    #endregion
}
