using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using SourceConsole;

public class PositionRecord {
    public Vector2 position;
    public float time;

    public float timeSinceRecord {
        get {
            return Time.time - time;
        }
    }
    
    public float DistanceFromRecord(Transform transform) {
        return Vector2.Distance(position, transform.position);
    }

    public float DistanceFromRecord(Vector2 position) {
        return Vector2.Distance(this.position, position);
    }

    public PositionRecord(Vector2 position) {
        this.position = position;
        time = Time.time;
    }

    public PositionRecord(Vector2 position, float timeRecorded) {
        this.position = position;
        this.time = timeRecorded;
    }
}

class FlipAchievementStage {
    public enum Stage { Stabilised, FlippedLeft, FlippedRight }
    public Stage lastStage = Stage.Stabilised;
    public Stage currentStage = Stage.Stabilised;
    public float lastModifiedStage;

    public void ChangeStage(Stage newStage) {
        lastModifiedStage = Time.time;
        lastStage = currentStage;
        currentStage = newStage;
    }
}

public class PlayerShipController : MonoBehaviour {
    public static PlayerShipController Singletron;
    public static List<PlayerShipController> Ships = new List<PlayerShipController>();

    private static bool AchievedHover10 = false;
    private static bool AchievedHover20 = false;
    private static bool AchievedHover30 = false;
    private static bool AchievedOneFlip = false;
    
    [Header("Variables")]
    public float velocityViewOffsetModifier = 1;
    [Header("Player name and other texts")]
    public TMP_Text playerName;
    public TMP_Text gotAchievement;
    [Header("Main ship sprite body renderer")]
    public SpriteRenderer bodySpriteRenderer;
    [Header("Main ship collider")]
    public PolygonCollider2D bodyPolygonCollider2D;
    [Header("The exploded ship container gameObject")]
    public GameObject explodedShip;
    [Header("Components")]
    public EngineController leftEngine;
    public EngineController rightEngine;
    public LegController leftLeg;
    public LegController rightLeg;

    [HideInInspector]
    public float lastTouchedLevel;
    [HideInInspector]
    public List<PositionRecord> positionsRecorded;
    [HideInInspector]
    public float lastTurnedOnEngines;
    [HideInInspector]
    public bool developerMode = false;
    [HideInInspector]
    public bool hasExploded = false;

    [HideInInspector]
    public Rigidbody2D selfRigidbody;
    [HideInInspector]
    public NetworkObject networkObject;
    [HideInInspector]
    public CrateController carriedCrate;
    /// <summary>
    /// The crate carried by our LOCAL player ship (this uses Singletron)
    /// </summary>
    public static CrateController CarriedCrate {
        get {
            if (Singletron == null) return null;
            return Singletron.carriedCrate;
        }
        set {
            if (Singletron == null) return;
            Singletron.carriedCrate = value;
        }
    }

    private FlipAchievementStage flipAchStage = new FlipAchievementStage();
    private Vector2 lastCheckedPosition;

    private int ghostReplayId = -1;

    private void OnEnable() {
        if (Ships == null) Ships = new List<PlayerShipController>();

        Ships.Add(this);

        UpdateUIShipSkinImage.SkinChanged += LocalSkinChanged;
    }

    private void OnDestroy() {
        UpdateUIShipSkinImage.SkinChanged -= LocalSkinChanged;

        if(CountPlayerObjects() == 0) {
            DeathMenuController.OpenMenu();
        }
    }

    private void LocalSkinChanged() {
        bodySpriteRenderer.sprite = ShipSkinsManager.Skins[ShipSkinsManager.SelectedSkin].GetSprite();
    }

    private void Awake() {
        selfRigidbody = GetComponent<Rigidbody2D>();
        networkObject = gameObject.GetNetworkComponent();

        if (NetworkingManager.CurrentLobbyValid && networkObject != null && networkObject.IsMine() || !NetworkingManager.CurrentLobbyValid) {
            positionsRecorded = new List<PositionRecord>();
        }
    }

    private void AssignVictoryMenuComponents() {
        VictoryMenuController.Singletron.leftEngine = leftEngine;
        VictoryMenuController.Singletron.rightEngine = rightEngine;
        VictoryMenuController.Singletron.leftLeg = leftLeg;
        VictoryMenuController.Singletron.rightLeg = rightLeg;
    }

    private void Start() {
        MainCameraController.AddTarget(this);
        MainCameraController.ForcePosition();

        if (Time.time == 0) return;

        ghostReplayId = GhostReplayRecorder.Singleton.GetCurrentGhostReplay().GetNewGhostReplayId();

        #region Setting the ship's skin
        if (NetworkingManager.CurrentLobbyValid && SceneManager.GetActiveScene().name != "Main Menu") { //we are not in a lobby
            int selectedSkinIndex = int.Parse(SteamMatchmaking.GetLobbyMemberData((CSteamID)NetworkingManager.CurrentLobby, (CSteamID)networkObject.owner, "selectedShipSkin"));

            bodySpriteRenderer.sprite = ShipSkinsManager.Skins[selectedSkinIndex].GetSprite();
            
            GhostReplayRecorder.AddInitialPlayerData(ghostReplayId, selectedSkinIndex, SteamFriends.GetFriendPersonaName((CSteamID)networkObject.owner));
        } else {
            GhostReplayRecorder.AddInitialPlayerData(ghostReplayId, ShipSkinsManager.SelectedSkin, SteamFriends.GetPersonaName()); //if not in coop
            LocalSkinChanged();
        }
        #endregion

        if (LevelLoader.IsPlayingEventLevel()) {
            bodySpriteRenderer.sprite = Resources.Load<Sprite>("Ship Skins/mainShip" + GameEvents.GetCurrentEvent().name); //we dont have to check if GetCurrentEvent is valid since it has to be in order for us to be playing an event level
        }

        if (NetworkingManager.CurrentLobbyValid && networkObject != null) {
            networkObject.enableSending = networkObject.IsMine();

            if (networkObject.IsMine()) {
                Singletron = this;
            }

            playerName.text = SteamFriends.GetFriendPersonaName((CSteamID)networkObject.owner);
        } else {
            Singletron = this;
            playerName.text = "";
        }

        if (VictoryMenuController.Singletron != null) {
            if(!SteamManager.Initialized || !NetworkingManager.CurrentLobbyValid) {
                AssignVictoryMenuComponents();
            } else {
                if (networkObject.IsMine()) {
                    AssignVictoryMenuComponents();
                }
            }
        }
    }

    private void OnDisable() {
        Ships.Remove(this);

        MainCameraController.RemoveTarget(this);

        if (!hasExploded)
        {
            GenerateAndAddGhostReplaySnapshot();
        }
    }

    private float lastAng = 0;
    private float lastGotFlipAchievement;

    private void GenerateAndAddGhostReplaySnapshot()
    {
        var newSnapshot = new GhostReplayPlayerSnapshot();

        newSnapshot.id = ghostReplayId;

        newSnapshot.x = selfRigidbody.position.x;
        newSnapshot.y = selfRigidbody.position.y;
        newSnapshot.rotation = selfRigidbody.rotation;

        newSnapshot.leftEngine = leftEngine.GetHealth();
        newSnapshot.leftLeg = leftLeg.GetHealth();
        newSnapshot.rightLeg = rightLeg.GetHealth();
        newSnapshot.rightEngine = rightEngine.GetHealth();

        newSnapshot.leftEngineOn = leftEngine.particlesEmissions.enabled;
        newSnapshot.rightEngineOn = rightEngine.particlesEmissions.enabled;

        GhostReplayRecorder.AddPlayerSnapshot(newSnapshot);
    }

    private void Update() {
        /*Vector2 distance = (Vector2)transform.position - lastCheckedPosition;
        lastCheckedPosition = transform.position; //distance travelled*/

        if (!hasExploded)
        {
            GenerateAndAddGhostReplaySnapshot();
        }
        
        if (SceneManager.GetActiveScene().name == "Game Level" && (NetworkingManager.CurrentLobbyValid && networkObject != null && networkObject.IsMine() || !NetworkingManager.CurrentLobbyValid)) {
            #region Enabling/disabling developer mode
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (IsUserDebugger.GetIsUserDebugger()) {
                    developerMode = !developerMode;

                    if (developerMode) {
                        transform.eulerAngles = Vector3.zero;
                        selfRigidbody.bodyType = RigidbodyType2D.Static;
                        selfRigidbody.bodyType = RigidbodyType2D.Kinematic;
                    } else {
                        selfRigidbody.bodyType = RigidbodyType2D.Dynamic;
                    }
                } else {
                    developerMode = false;
                }
            }
            #endregion

            #region Distance recording
            if (positionsRecorded == null) positionsRecorded = new List<PositionRecord>();
            positionsRecorded.Add(new PositionRecord(transform.position));

            List<int> indexesToRemove = new List<int>();
            PositionRecord position10 = new PositionRecord(transform.position);
            PositionRecord position20 = new PositionRecord(transform.position);
            PositionRecord position30 = new PositionRecord(transform.position);

            for(int i = 0; i < positionsRecorded.Count; i++) {
                PositionRecord record = positionsRecorded[i];
                float timeSinceRecorded = record.timeSinceRecord;

                if (timeSinceRecorded > 10 && timeSinceRecorded <= 11) position10 = record;
                if (timeSinceRecorded > 20 && timeSinceRecorded <= 21) position20 = record;
                if (timeSinceRecorded > 30 && timeSinceRecorded <= 31) position30 = record;

                if (Time.time - record.time > 30) indexesToRemove.Add(i);
            }

            foreach(int index in indexesToRemove) {
                positionsRecorded.RemoveAt(index);
            }
            indexesToRemove.Clear();

            float standingStillDistance = 0.3f;

            //print(Time.time - lastTouchedLevel);
            
            if (Time.time - lastTouchedLevel >= 9 && !AchievedHover10 && position10.timeSinceRecord >= 9 && position10.DistanceFromRecord(transform) < standingStillDistance) {
                print("got HOVER_10");
                SteamCustomUtils.SetAchievement("HOVER_10");
                AchievedHover10 = true;
            }
            if (Time.time - lastTouchedLevel >= 19 && !AchievedHover20 && position20.timeSinceRecord >= 19 && position20.DistanceFromRecord(transform) < standingStillDistance) {
                print("got HOVER_20");
                SteamCustomUtils.SetAchievement("HOVER_20");
                AchievedHover20 = true;
            }
            if (Time.time - lastTouchedLevel >= 29 && !AchievedHover30 && position30.timeSinceRecord >= 29 && position30.DistanceFromRecord(transform) < standingStillDistance) {
                print("got HOVER_30");
                SteamCustomUtils.SetAchievement("HOVER_30");
                AchievedHover30 = true;
            }
            #endregion

            #region Angle velocity recording
            float ang = transform.eulerAngles.z;
            ang %= 360;
            if (ang > 180) ang -= 360;

            //code for if the player has flipped over to the left side
            if(lastAng < -90 && ang >= -90) {
                flipAchStage.ChangeStage(FlipAchievementStage.Stage.FlippedLeft);
            }

            if (lastAng < -5 && ang >= -5 && flipAchStage.currentStage == FlipAchievementStage.Stage.FlippedLeft && Time.time - flipAchStage.lastModifiedStage < 2) {
                flipAchStage.ChangeStage(FlipAchievementStage.Stage.Stabilised);
                if (Time.time - lastTurnedOnEngines < 2) {
                    if (!AchievedOneFlip) {
                        AchievedOneFlip = true;
                        lastGotFlipAchievement = Time.time;
                        SteamCustomUtils.SetAchievement("ONE_FLIP");
                    } else {
                        if (Time.time - lastGotFlipAchievement < 3.3f) {
                            SteamCustomUtils.SetAchievement("TWO_FLIPS");
                        }
                    }
                }
            }

            //code if the player has flipped over to the right side
            if (lastAng > -90 && ang <= -90) {
                flipAchStage.ChangeStage(FlipAchievementStage.Stage.FlippedRight);
            }

            if (lastAng < 5 && ang >= 5 && flipAchStage.currentStage == FlipAchievementStage.Stage.FlippedRight && Time.time - flipAchStage.lastModifiedStage < 2) {
                flipAchStage.ChangeStage(FlipAchievementStage.Stage.Stabilised);
                if (Time.time - lastTurnedOnEngines < 2) {
                    if (!AchievedOneFlip) {
                        AchievedOneFlip = true;
                        lastGotFlipAchievement = Time.time;
                        SteamCustomUtils.SetAchievement("ONE_FLIP");
                    } else {
                        if (Time.time - lastGotFlipAchievement < 3.3f) {
                            SteamCustomUtils.SetAchievement("TWO_FLIPS");
                        }
                    }
                }
            }

            lastAng = ang;
            #endregion
        }

        if (DeathMenuController.Singletron != null && (leftEngine.GetHealth() == ShipComponentController.ComponentHealth.Destroyed && rightEngine.GetHealth() == ShipComponentController.ComponentHealth.Destroyed)) {
            if (networkObject == null) {
                DeathMenuController.OpenMenu();
            } else {
                MainCameraController.RemoveTarget(this);

                if (CountPlayerObjects() == 0) { //if all players died
                    DeathMenuController.OpenMenu();
                }
            }
        }
    }

    [ConCommand("player_explode")]
    public static void ForceExplosion()
    {
        if (Singletron == null) return;

        if (Singletron.hasExploded)
        {
            SourceConsole.SourceConsole.print("Player's ship has already exploded!");
            return;
        }

        SpaceshipHealthStatusController.UpdateShipBodyHealth(ShipComponentController.ComponentHealth.Destroyed);

        Singletron.NetworkComponentDamaged(4, true);
        NetworkingManager.SendPacket(new object[] { 4, 4, true }, 1);
    }

    private void FixedUpdate() {
        //lerp achievement get text
        Color textColor = gotAchievement.color;
        textColor.a = Mathf.Lerp(textColor.a, 0, 0.0165f);
        gotAchievement.color = textColor;

        if (developerMode) {
            //lateral movement
            float movementSpeed = 0.06f;
            
            if (Input.GetKey(KeyCode.W)) selfRigidbody.position = selfRigidbody.position + Vector2.up * movementSpeed;
            if (Input.GetKey(KeyCode.S)) selfRigidbody.position = selfRigidbody.position + Vector2.down * movementSpeed;
            if (Input.GetKey(KeyCode.D)) selfRigidbody.position = selfRigidbody.position + Vector2.right * movementSpeed;
            if (Input.GetKey(KeyCode.A)) selfRigidbody.position = selfRigidbody.position + Vector2.left * movementSpeed;

            //rotating
            float rotationSpeed = 2.5f;

            if (Input.GetKey(KeyCode.Q)) selfRigidbody.rotation += rotationSpeed;
            if (Input.GetKey(KeyCode.E)) selfRigidbody.rotation -= rotationSpeed;
        }
    }

    public static int CountPlayerObjects() {
        int count = 0;

        foreach(var ship in Ships)
        {
            if (!ship.hasExploded) count++;
        }

        return count;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (networkObject != null && !networkObject.IsMine()) return;


        //if ship is not controlled by local player, return and halt method
        /*if(collision.relativeVelocity.magnitude > 40) {
            ExplosionsController.CreateExplosion(0, transform.position);
            MainCameraController.StartShake(1, 0.4f);
            Destroy(gameObject);

            int countOfPlayerShipsLeft = CountPlayerObjects() - 1; //minus one player cause we just deleted one and unity hierachy only gets updated at next update or someting, aint noone got time for dat
            if (countOfPlayerShipsLeft == 0) {
                DeathMenuController.OpenMenu();
            }
        }*/

        float minimumForceRequiredForExplosion = 3;
        if(leftEngine.GetHealth() == ShipComponentController.ComponentHealth.Destroyed && rightEngine.GetHealth() == ShipComponentController.ComponentHealth.Destroyed)
        {
            minimumForceRequiredForExplosion = 1;
        }

        if (collision.relativeVelocity.magnitude > minimumForceRequiredForExplosion)
        {
            if (leftLeg.GetHealth() == ShipComponentController.ComponentHealth.Destroyed && rightLeg.GetHealth() == ShipComponentController.ComponentHealth.Destroyed)
            {
                if (leftEngine.GetHealth() == ShipComponentController.ComponentHealth.Destroyed || rightEngine.GetHealth() == ShipComponentController.ComponentHealth.Destroyed)
                {
                    SpaceshipHealthStatusController.UpdateShipBodyHealth(ShipComponentController.ComponentHealth.Destroyed);

                    NetworkComponentDamaged(4, true);
                    NetworkingManager.SendPacket(new object[] { 4, 4, true }, 1);
                }
            }
        }
    }

    /// <summary>
    /// if isDamaged is false, than the component was completely destroyed
    /// </summary>
    /// <param name="componentId"></param>
    /// <param name="isDamaged"></param>
    public void NetworkComponentDamaged(int componentId, bool isDamaged) {
        if(componentId == 0) { //id 0 is the left landing leg
            if (isDamaged) {
                Destroy(leftLeg.bottomFixedJoint);
            } else {
                Destroy(leftLeg.topFixedJoint);
            }
        }

        if (componentId == 1) { //id 1 is the right landing leg
            if (isDamaged) {
                Destroy(rightLeg.bottomFixedJoint);
            } else {
                Destroy(rightLeg.topFixedJoint);
            }
        }

        if (componentId == 2) { //id 2 is the left engine
            if (isDamaged) {
                leftEngine.DoDamagedEffect();
            } else {
                leftEngine.DoDestroyedEffect();
            }
        }

        if (componentId == 3) { //id 3 is the right engine
            if (isDamaged) {
                rightEngine.DoDamagedEffect();
            } else {
                rightEngine.DoDestroyedEffect();
            }
        }

        if(componentId == 4 && !hasExploded) //id 4 is the whole ship itself
        {
            hasExploded = true;

            playerName.SetText("");

            bodySpriteRenderer.enabled = false;
            bodyPolygonCollider2D.enabled = false;
            explodedShip.SetActive(true);

            leftEngine.DoDestroyedEffect();
            rightEngine.DoDestroyedEffect();
            if(leftLeg.topFixedJoint != null) Destroy(leftLeg.topFixedJoint);
            if (leftLeg.bottomFixedJoint != null) Destroy(leftLeg.bottomFixedJoint);
            if (rightLeg.topFixedJoint != null) Destroy(rightLeg.topFixedJoint);
            if (rightLeg.bottomFixedJoint != null) Destroy(rightLeg.bottomFixedJoint);

            MainCameraController.RemoveTarget(this);

            StartCoroutine(PushExplodedShipPartsApart());

            Ships.Remove(this);

            //dont need to send data for a ship that's dead
            if(networkObject != null) networkObject.enableSending = false;

            if (IsMine())
            {
                SpaceshipHealthStatusController.UpdateShipBodyHealth(ShipComponentController.ComponentHealth.Destroyed);
            }

            if (CountPlayerObjects() <= 0)
            {
                DeathMenuController.OpenMenu();
            }
        }
    }

    private IEnumerator PushExplodedShipPartsApart()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        var explodedShipRigidbodies = explodedShip.GetComponentsInChildren<Rigidbody2D>();
        
        for (int i = 0; i < explodedShipRigidbodies.Length; i++)
        {
            Rigidbody2D rigidbody = explodedShipRigidbodies[i];

            MainCameraController.AddNonPlayerTarget(rigidbody.transform);

            var otherRigidbody = explodedShipRigidbodies[(i + 1) % explodedShipRigidbodies.Length];

            var explosionDirection = (rigidbody.position - otherRigidbody.position).normalized * 2.5f;

            rigidbody.velocity = selfRigidbody.velocity + (Vector2)explosionDirection;
            rigidbody.angularVelocity = selfRigidbody.angularVelocity;

            var spriteRenderer = rigidbody.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = this.bodySpriteRenderer.sprite;
        }
    }

    public void OverrideEngineParticles(int engineId, bool status) {
        if (engineId == 0) { //id 0 is the left engine
            leftEngine.OverrideParticlesEmission(status);
        } else { //id 1 is the right engine
            rightEngine.OverrideParticlesEmission(status);
        }
    }

    public void ShowAchievementGet(string name) {
        string displayName = SteamCustomUtils.GetAchievementName(name);

        gotAchievement.color = Color.white;
        gotAchievement.text = "Got achievement '" + displayName + "'!";
    }

    public static PlayerShipController GetClosestShip(Vector2 target) {
        PlayerShipController closest = null;

        if (Ships.Count == 1) return Ships[0];

        foreach(PlayerShipController ship in Ships) {
            if (closest == null || Vector2.Distance(ship.transform.position, target) < Vector2.Distance(closest.transform.position, target)) closest = ship;
        }

        return closest;
    }

    public static PlayerShipController GetShipBySteamId(ulong steamId) {
        if (Ships.Count == 0) return null;

        foreach(PlayerShipController ship in Ships) {
            if (ship.networkObject != null && ship.networkObject.owner == steamId) return ship;
        }

        return null;
    }

    public bool IsMine()
    {
        if (networkObject == null)
        {
            return true;
        }
        else
        {
            return networkObject.IsMine();
        }
    }
}
