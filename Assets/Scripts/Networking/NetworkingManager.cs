using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;
using System.Text;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using VoteType = CoopVoteController.VoteType;
using Rope;

public static class LobbyHelperFunctions {
    public static List<ulong> GetAllLobbyMembers() {
        List<ulong> members = new List<ulong>();
        if (!NetworkingManager.CurrentLobbyValid) return members;

        int numOfMembers = SteamMatchmaking.GetNumLobbyMembers((CSteamID) NetworkingManager.CurrentLobby);
        for(int i = 0; i < numOfMembers; i++) {
            members.Add((ulong) SteamMatchmaking.GetLobbyMemberByIndex((CSteamID) NetworkingManager.CurrentLobby, i));
        }

        return members;
    }
}

public static class NetworkingExtensions {
    public static NetworkObject GetNetworkComponent(this GameObject gameObject) {
        return gameObject.GetComponent<NetworkObject>();
    }
}

public class InstantiationQueueItem {
    public int id;
    /// <summary>
    /// The steam id of the player who wants to instantiate this object
    /// </summary>
    public ulong owner;
    public string prefabName;
    public Vector2 position;
    public float angle;

    public InstantiationQueueItem(int id, ulong spawner, string prefabName, Vector2 position, float angle) {
        this.id = id;
        this.owner = spawner;
        this.prefabName = prefabName;
        this.position = position;
        this.angle = angle;
    }
}

public class NetworkingManager : MonoBehaviour {
    public static ulong CurrentLobby = 0;
    public static bool CurrentLobbyValid {
        get {
            return CurrentLobby != 0;
        }
    }
    public static Action<ulong> LobbyJoined;
    public static Action<ulong> LobbyCreated;
    public static Action<ulong> OnLobbyMemberDataUpdated;
    public static Action LobbyLeft;

    public static Dictionary<int, NetworkObject> NetworkedObjects = new Dictionary<int, NetworkObject>();
    
    private static NetworkingManager Singletron;

    private int NextNetworkObjectId = 0;

    private bool sceneLoaded = false;
    private List<InstantiationQueueItem> InstantiationQueue = new List<InstantiationQueueItem>();

    private void Awake() {
        if(Singletron != null) {
            Destroy(gameObject);
            return;
        }

        Singletron = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;

        //channel zero is lobby stuff
        //channel one is in-game stuff
    }

    private void OnEnable() {
        if (SteamManager.Initialized) {
            SteamCallbacks.P2PSessionRequest_t.RegisterCallback(OnP2PSessionRequest);
            SteamCallbacks.P2PSessionConnectFail_t.RegisterCallback(OnP2PSessionConnectFail);
            SteamCallbacks.LobbyEnter_t.RegisterCallback(JoinedLobbyAPI);
            SteamCallbacks.GameLobbyJoinRequested_t.RegisterCallback(GameLobbyJoinRequestedAPI);
            SteamCallbacks.LobbyChatUpdate_t.RegisterCallback(LobbyChatUpdateAPI);
            SteamCallbacks.PersonaStateChange_t.RegisterCallback(OnLobbyMemberPersonaChangeAPI);
        }
        
        LobbyJoined += JoinedLobby;
        LobbyCreated += CreatedLobby;
    }

    private void OnDisable() {
        SteamCallbacks.P2PSessionRequest_t.UnregisterCallback(OnP2PSessionRequest);
        SteamCallbacks.P2PSessionConnectFail_t.UnregisterCallback(OnP2PSessionConnectFail);
        SteamCallbacks.LobbyEnter_t.UnregisterCallback(JoinedLobbyAPI);
        SteamCallbacks.PersonaStateChange_t.UnregisterCallback(OnLobbyMemberPersonaChangeAPI);

        LobbyJoined -= JoinedLobby;
        LobbyCreated -= CreatedLobby;
    }

    private void OnLobbyMemberPersonaChangeAPI(PersonaStateChange_t callback) {
        if((ulong)SteamUser.GetSteamID() == callback.m_ulSteamID || LobbyMenuController.GetOtherPlayer() == callback.m_ulSteamID) {
            if (OnLobbyMemberDataUpdated != null) OnLobbyMemberDataUpdated(callback.m_ulSteamID);
        }
    }

    private void GameLobbyJoinRequestedAPI(GameLobbyJoinRequested_t callback) {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void LobbyChatUpdateAPI(LobbyChatUpdate_t callback) {
        if (callback.m_ulSteamIDLobby != CurrentLobby) return;

        EChatMemberStateChange change = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;

        if (callback.m_ulSteamIDMakingChange == SteamUser.GetSteamID().m_SteamID) { //if we are the player making the change
            if (change == EChatMemberStateChange.k_EChatMemberStateChangeEntered) {
                if (LobbyJoined != null) LobbyJoined(callback.m_ulSteamIDLobby);
            } else {
                if (LobbyLeft != null) LobbyLeft();
            }
        } else { //if we are not the player making the change
            if(change == EChatMemberStateChange.k_EChatMemberStateChangeEntered && !Application.isFocused) {
                UISoundEmitter.PlaySound(2);
            }
        }
    }

    private void JoinedLobbyAPI(LobbyEnter_t callback) {
        CurrentLobby = callback.m_ulSteamIDLobby;
        if (LobbyJoined != null) LobbyJoined(callback.m_ulSteamIDLobby);
    }

    public static void UpdateEventProgressData() {
        if (!SteamManager.Initialized || !CurrentLobbyValid) return;

        GameEvent currentEvent = GameEvents.GetCurrentEvent();
        if (currentEvent != null) {
            SteamMatchmaking.SetLobbyMemberData((CSteamID)CurrentLobby, "eventProgress", GameEvents.CurrentEventCoopProgression.ToString());
        } else {
            SteamMatchmaking.SetLobbyMemberData((CSteamID)CurrentLobby, "eventProgress", "0");
        }
    }

    private void JoinedLobby(ulong lobbyId) {
        ResetNetwork();

        LevelLoader.GravityEnabled = true;

        SendPacketOtherOnly(new object[] { }, 2); //sends an empty 'handshake' packet

        //reseting some basic member data variables
        SteamMatchmaking.SetLobbyMemberData((CSteamID)lobbyId, "selectedShipSkin", ShipSkinsManager.SelectedSkin.ToString());

        UpdateEventProgressData();
    }

    private void CreatedLobby(ulong lobbyId) {
        ResetNetwork();
    }

    private void SpawnInstantiatedObject(int id, ulong owner, string prefabName, Vector2 pos, float angle) {
        GameObject nObject = Instantiate(Resources.Load<GameObject>(prefabName), pos, Quaternion.Euler(0, 0, angle));
        nObject.GetComponent<Renderer>().enabled = false;
        NetworkObject networkComponent = nObject.GetComponent<NetworkObject>();

        if (networkComponent == null) {
            Debug.LogWarning("Spawned object has no network component! Adding new one");
            networkComponent = nObject.AddComponent<NetworkObject>();
        }

        print(string.Format("instantiating prefab '{0}' with id {1} from {2} at position {3} and angle {4}", id, owner, prefabName, pos, angle));

        nObject.transform.position = pos;
        nObject.transform.eulerAngles = new Vector3(0, 0, angle);

        networkComponent.id = id;
        networkComponent.owner = owner; //owner is the player who sent this packet

        NetworkedObjects.Add(id, networkComponent);

        nObject.GetComponent<Renderer>().enabled = true;
    }

    public static void SetSceneNotLoaded() {
        if (Singletron == null) return;

        Singletron.sceneLoaded = false;
    }

    public static void ResetNetwork() {
        if (Singletron == null) return;

        print("reset network stuff");
        Singletron.InstantiationQueue.Clear();
        NetworkedObjects.Clear();
        if(Singletron.NextNetworkObjectId > 50) {
            Singletron.NextNetworkObjectId = 0;
        }

        CrateController.Rope_Enable = false;

        RopeManager.ResetRopeId();
        GhostReplayRecorder.NextGhostId = 0;

        LaunchingPadController.LaunchPads.Clear();
        LaunchingPadController.OwnerLaunchPadIndex = UnityEngine.Random.Range(0, 2);
    }

    private void OnSceneChanged(Scene current, Scene next) {
        sceneLoaded = false;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if(scene.name == "Game Level" && CurrentLobbyValid) { //if we finally loaded into the game level scene and we are in coop, we start clearing out the instantiation queue
            sceneLoaded = true;
        }
    }

    private float LastReadP2P;

    private void Update() {
        if (!SteamManager.Initialized) return;

        if (SceneManager.GetActiveScene().name == "Game Level" && CurrentLobbyValid && sceneLoaded && SceneManager.GetActiveScene().isLoaded) {
            foreach (InstantiationQueueItem queueItem in InstantiationQueue) {
                print(string.Format("queue item is " + queueItem.prefabName + " by " + queueItem.owner + " with id " + queueItem.id));
                SpawnInstantiatedObject(queueItem.id, queueItem.owner, queueItem.prefabName, queueItem.position, queueItem.angle);
            }

            InstantiationQueue.Clear();
        }

        if (Time.time - LastReadP2P > 1f / 60f) {
            LastReadP2P = Time.time;

            for(int i = 0; i < 2; i++) {
                while (ReadRawP2P(i)) {
                    // Nothing here.
                }
            }
        }
    }

    private unsafe bool ReadRawP2P(int channel) {
        uint dataAvailable = 0;

        if (!SteamNetworking.IsP2PPacketAvailable(out dataAvailable, channel)) return false;

        byte[] data = new byte[dataAvailable + 1024];
        
        fixed (byte* p = data) {
            CSteamID steamId;
            if (!SteamNetworking.ReadP2PPacket(data, dataAvailable, out dataAvailable, out steamId, channel) || dataAvailable == 0) return false;

            P2PData((ulong)steamId, data, (int)dataAvailable, channel);
            return true;
        }
    }

    public static byte[] ObjectsToBytes(object[] objects) {
        using(MemoryStream ms = new MemoryStream()) {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, objects);

            byte[] bytes = ms.ToArray();
            ms.Flush();

            //print("converted objects to bytes, bytes length is " + bytes.Length);

            return bytes;
        }
    }

    public static object[] BytesToObjects(byte[] bytes, int length) {
        BinaryFormatter bf = new BinaryFormatter();

        //print("converted bytes to objects, bytes received length is " + bytes.Length);

        using (MemoryStream ms = new MemoryStream(bytes, 0, length)) {
            object dataObject = bf.Deserialize(ms);

            return (object[])dataObject;
        }
    }

    private int GetNextAvailableObjectId() {
        int lowestId = 0;

        foreach(KeyValuePair<int, NetworkObject> pair in NetworkedObjects) {
            if (lowestId <= pair.Key) lowestId = pair.Key + 1;
        }

        return lowestId;
    }

    private void P2PData(ulong steamId, byte[] bytesData, int length, int channel) {
        object[] data = BytesToObjects(bytesData, length);

        /*print("received data is:");
        foreach(object obj in data) {
            print(obj.ToString());
        }*/

        if (channel == 0) { //channel zero is lobby stuff
            if((int)data[0] == 0) { //ready to start game
                //this byte signifies we are ready to head into the Game Level scene
                //since both players have to agree to the same level in order to start the game, we can simply take the owner's selected level and switch to that

                if (!Application.isFocused) {
                    UISoundEmitter.PlaySound(1);
                }

                string gameLevel = SteamMatchmaking.GetLobbyMemberData((CSteamID)CurrentLobby, SteamMatchmaking.GetLobbyOwner((CSteamID) CurrentLobby), "levelvoted");

                if(gameLevel == "event") {
                    GameEvent currentEvent = GameEvents.GetCurrentEvent();
                    ulong otherPlayer = LobbyMenuController.GetOtherPlayer();
                    int otherPlayerProgress = 0;
                    int.TryParse(SteamMatchmaking.GetLobbyMemberData((CSteamID)CurrentLobby, (CSteamID)otherPlayer, "eventProgress"), out otherPlayerProgress);
                    int nextLevel = Mathf.Min(otherPlayerProgress, GameEvents.CurrentEventCoopProgression);

                    LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.Game, string.Format("{0}/{0}_mp{1}", currentEvent.name, nextLevel));
                    SceneManager.LoadScene("Game Level");
                } else {
                    ulong levelWorkshopId;
                    if (ulong.TryParse(gameLevel, out levelWorkshopId)) {
                        ulong sizeOnDisk;
                        string path;
                        uint timestamp;
                        if (SteamUGC.GetItemInstallInfo(new PublishedFileId_t(levelWorkshopId), out sizeOnDisk, out path, 2000, out timestamp)) {
                            LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.External, path + "/" + levelWorkshopId.ToString() + ".level");
                            SceneManager.LoadScene("Game Level");
                        }
                    } else {
                        LevelLoader.SetLevelDirectory(LevelLoader.LevelOrigin.Game, gameLevel);
                        SceneManager.LoadScene("Game Level");
                    }
                }
            }

            if((int)data[0] == 1) { //kick off lobby message
                if ((ulong)SteamMatchmaking.GetLobbyOwner((CSteamID)CurrentLobby) == steamId) { //if the person who sent us this packet is the lobby owner
                    PromptsController.OpenErrorPrompt("Kicked from lobby by " + SteamFriends.GetFriendPersonaName((CSteamID)steamId));
                    SteamMatchmaking.LeaveLobby((CSteamID)CurrentLobby); //we gotta go!
                    CurrentLobby = 0;
                    if (LobbyLeft != null) LobbyLeft();
                }
            }

            if((int)data[0] == 2) //sync Rope_Enable convar
            {
                bool enabled = (bool)data[1];

                SourceConsole.SourceConsole.ExecuteString($"rope_enable {enabled}");
            }

            if ((int)data[0] == 3) //sync Rope_Enable convar
            {
                bool enabled = (bool)data[1];

                SourceConsole.SourceConsole.ExecuteString($"rope_hinge {enabled}");
            }
        }

        if (channel == 1)
        { //channel one is in-game stuff
            //this networking system/scheme is based of Photon's
            //first data is the type of in-game event
            if ((int)data[0] == 0)
            { //a player wants to instantiate an object
                if ((ulong)SteamMatchmaking.GetLobbyOwner((CSteamID)CurrentLobby) == SteamUser.GetSteamID().m_SteamID)
                {
                    List<object> originalDataWithoutPacketId = data.ToList();
                    originalDataWithoutPacketId.RemoveAt(0); //remove the old message code

                    List<object> newSendData = new List<object>();
                    newSendData.Add(1); //the new relayed packet id is 1
                    newSendData.Add(steamId);
                    newSendData.Add(NextNetworkObjectId); //we add the new object's network id
                    newSendData.AddRange(originalDataWithoutPacketId); //adding the rest of the original data minus the original packet id (which was zero, now it's one)

                    NextNetworkObjectId++;

                    SendPacket(newSendData.ToArray(), 1, EP2PSend.k_EP2PSendReliableWithBuffering);
                }
            }

            if ((int)data[0] == 1)
            { //object is getting instantiated with specific id
                ulong ownerSteamId = (ulong)data[1];
                int objectId = (int)data[2];
                string prefabName = (string)data[3];
                float posX = (float)data[4];
                float posY = (float)data[5];
                float angle = (float)data[6];

                /*if(prefabName == "Missile/Missile") {
                    print("instantiating missile, props are:");
                    for(int i = 0; i < data.Length; i++) {
                        print("[" + i + "] = " + data[i]);
                    }
                }*/

                print("got instantiation packet from " + ownerSteamId + " - adding to queue");
                InstantiationQueue.Add(new InstantiationQueueItem(objectId, ownerSteamId, prefabName, new Vector2(posX, posY), angle));
            }

            if ((int)data[0] == 2)
            { //object destroyed
                int objectId = (int)data[1]; //the uid of the networked object

                if (NetworkedObjects.ContainsKey(objectId))
                {
                    if (NetworkedObjects[objectId] != null && NetworkedObjects[objectId].gameObject != null) Destroy(NetworkedObjects[objectId].gameObject);
                    NetworkedObjects.Remove(objectId);
                }
            }

            if ((int)data[0] == 3 & sceneLoaded)
            { //object moved (unless we were the one who sent the update packet)
                int objectId = (int)data[1];
                float posX = (float)data[2];
                float posY = (float)data[3];
                float angle = (float)data[4];

                if (NetworkedObjects.ContainsKey(objectId) && NetworkedObjects[objectId] != null)
                {
                    NetworkObject nObject = NetworkedObjects[objectId];

                    Vector2 networkPosition = new Vector2(posX, posY);

                    //adding the new state
                    NetworkObject.State newState;
                    newState.angle = angle;
                    newState.time = Time.time;
                    newState.pos = networkPosition;
                    nObject.AddState(newState);
                }
                else
                {
                    Debug.LogWarning("Trying to update position for a network object that does not exist! (id = " + objectId + ")");
                }
            }

            if ((int)data[0] == 4)
            { //the health of a component of the other player's ship was changed
                int componentId = (int)data[1];
                bool isDamaged = (bool)data[2];

                PlayerShipController affectedShip = PlayerShipController.GetShipBySteamId(steamId); //gets the ship of the player who sent this packet
                if (affectedShip != null)
                {
                    Debug.Log($"component {componentId} with state {isDamaged} from player {steamId}", affectedShip.gameObject);

                    affectedShip.NetworkComponentDamaged(componentId, isDamaged);
                }
            }

            if ((int)data[0] == 5)
            { //when a player goes back to the lobby from within the game, he sends this packet in order to tell the other player to switch as well
                if (SceneManager.GetActiveScene().name == "Game Level")
                {
                    SceneManager.LoadScene("Main Menu");
                }
            }

            if ((int)data[0] == 6)
            { //called when a player's engines either turned on or off
                int engineId = (int)data[1];
                bool status = (bool)data[2]; //whether or not the engine is on

                PlayerShipController affectedShip = PlayerShipController.GetShipBySteamId(steamId); //gets the ship of the player who sent this packet
                if (affectedShip != null)
                {
                    affectedShip.OverrideEngineParticles(engineId, status);
                }
            }

            if ((int)data[0] == 7)
            { //when another player picks up a crate
                int crateId = (int)data[1];
                bool isPickedUp = (bool)data[2]; //whether or not the player picked up or dropped the specified crate

                CrateController crate = CrateController.GetCrateById(crateId);

                if (crate != null)
                {
                    PlayerShipController playerOwnedShip = PlayerShipController.GetShipBySteamId(steamId);
                    if (isPickedUp)
                    {
                        crate.AttachToPlayerShip(playerOwnedShip);
                    }
                    else
                    {
                        Vector2 dropOffLocation = new Vector2((float)data[3], (float)data[4]);
                        Vector2 dropOffVelocity = new Vector2((float)data[5], (float)data[6]);

                        crate.DetachFromPlayerShip(playerOwnedShip, dropOffLocation, dropOffVelocity);
                    }
                }
            }

            if ((int)data[0] == 8)
            { //player wants to start a vote about something
                VoteType voteType = (VoteType)((int)data[1]);

                CoopVoteController.DisplayNewVote(steamId, voteType);
            }

            if ((int)data[0] == 9)
            { //vote response
                bool agree = (bool)data[1];

                if (agree) SetSceneNotLoaded();

                CoopVoteController.VoteResponse(agree);
            }

            if ((int)data[0] == 10)
            { //player got achievement
                string achievementName = (string)data[1];

                PlayerShipController playerOwnedShip = PlayerShipController.GetShipBySteamId(steamId);
                if (playerOwnedShip != null)
                {
                    playerOwnedShip.ShowAchievementGet(achievementName);
                }
            }

            //capturing landing pad
            if ((int)data[0] == 11)
            {
                int padId = (int)data[1];
                bool isGettingCaptured = (bool)data[2];
                string levelName = (string)data[3];

                if (levelName == LevelLoader.GetLevelDirectory())
                {
                    LandingPadController landPad = LandingPadController.AllPads.Find(x => x.landPadId == padId);
                    if (landPad != null)
                    {
                        if (isGettingCaptured)
                        {
                            if (!landPad.isCapturing) landPad.lastStartedCapturing = Time.time;
                        }

                        landPad.isCapturing = isGettingCaptured;
                    }
                }
            }

            //owner telling other player where to spawn his ship
            if ((int)data[0] == 12)
            {
                Vector2 pos = new Vector2((float)data[1], (float)data[2]);
                float angle = (float)data[3];

                InstantiateObject("Player Ships/Player Ship", pos, angle);
            }

            //crate sensor forcefully attaching a crate to itself in no-gravity mode
            if((int)data[0] == 13)
            {
                int crateSensorId = (int)data[1];
                int crateId = (int)data[2];

                var crateSensor = CrateSensorController.GetCrateSensorById(crateSensorId);
                var crate = CrateController.GetCrateById(crateId);

                if(crateSensor != null && crate != null)
                {
                    crate.AttachToCrateSensor(crateSensor);
                }
            }
        }
    }

    private void OnP2PSessionRequest(P2PSessionRequest_t callback) {
        SteamNetworking.AcceptP2PSessionWithUser(callback.m_steamIDRemote);
    }

    private void OnP2PSessionConnectFail(P2PSessionConnectFail_t callback) { }

    /// <summary>
    /// Sends a packet to all players connected in lobby, including yourself
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length"></param>
    /// <param name="sendType"></param>
    /// <param name="channel"></param>
    public static void SendPacket(object[] data, int channel = 0, EP2PSend sendType = EP2PSend.k_EP2PSendReliable) {
        if (!SteamManager.Initialized) return;

        byte[] bytes = ObjectsToBytes(data);

        foreach (ulong user in LobbyHelperFunctions.GetAllLobbyMembers()) {
            SteamNetworking.SendP2PPacket((CSteamID)user, bytes, (uint)bytes.Length, sendType, channel);
        }
    }

    /// <summary>
    /// Same as `SendPacket`, but this only sends to other players in the lobby
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length"></param>
    /// <param name="channel"></param>
    /// <param name="sendType"></param>
    public static void SendPacketOtherOnly(object[] data, int channel = 0, EP2PSend sendType = EP2PSend.k_EP2PSendReliable) {
        if (!SteamManager.Initialized) return;

        byte[] bytes = ObjectsToBytes(data);
        ulong localPlayer = (ulong) SteamUser.GetSteamID();

        foreach (ulong user in LobbyHelperFunctions.GetAllLobbyMembers()) {
            if (user == localPlayer) continue;

            SteamNetworking.SendP2PPacket((CSteamID)user, bytes, (uint)bytes.Length, sendType, channel);
        }
    }

    public static void InstantiateObject(string prefabName, Vector2 position = new Vector2(), float angle = 0) {
        List<object> sendData = new List<object>();
        sendData.Add(0); //the type of packet
        sendData.Add(prefabName); //the prefab name
        sendData.Add(position.x); //position and angle data
        sendData.Add(position.y);
        sendData.Add(angle);

        byte[] bytes = ObjectsToBytes(sendData.ToArray());

        //SendPacket(sendData.ToArray(), sendData.Count, 1, EP2PSend.k_EP2PSendReliableWithBuffering);
        SteamNetworking.SendP2PPacket(SteamMatchmaking.GetLobbyOwner((CSteamID)CurrentLobby), bytes, (uint)bytes.Length, EP2PSend.k_EP2PSendReliableWithBuffering, 1);
    }
}
