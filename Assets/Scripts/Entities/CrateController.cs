using Rope;
using SourceConsole;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrateController : MonoBehaviour
{
    public static List<CrateController> Crates = new List<CrateController>();
    public static int NextCrateId;

    private static bool _RopeEnable = false;
    [ConVar]
    public static bool Rope_Enable
    {
        get { return _RopeEnable; }
        set
        {
            if (NetworkingManager.CurrentLobbyValid)
            {
                NetworkingManager.SendPacketOtherOnly(new object[] { 2, value });
            }

            _RopeEnable = value;
        }
    }

    private static bool _RopeHinge = false;
    [ConVar]
    public static bool Rope_Hinge
    {
        get { return _RopeHinge; }
        set
        {
            if (NetworkingManager.CurrentLobbyValid)
            {
                NetworkingManager.SendPacketOtherOnly(new object[] { 3, value });
            }

            _RopeHinge = value;
        }
    }

    public int crateId;

    private int currentRopeId = -1;

    private Rigidbody2D selfRigidbody;
    private bool isBeingCarried = false;
    private PlayerShipController carrierShip;
    private bool isForcefullyAttachedToSensor = false;

    private int ghostReplayId = -1;

    public static CrateController GetCrateById(int id)
    {
        foreach (CrateController crate in Crates)
        {
            if (crate.crateId == id) return crate;
        }

        return null;
    }

    private void Awake()
    {
        selfRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Trailer") ghostReplayId = GhostReplayRecorder.Singleton.GetCurrentGhostReplay().GetNewGhostReplayId();
    }

    private void OnEnable()
    {
        if (Crates == null) Crates = new List<CrateController>();

        Crates.Add(this);

        crateId = NextCrateId;
        NextCrateId++;
    }

    private void OnDisable()
    {
        Crates.Remove(this);
    }

    public bool GetIsBeingCarried()
    {
        return isBeingCarried;
    }

    public bool GetIsForcefullyAttachedToSensor()
    {
        return isForcefullyAttachedToSensor;
    }

    //used in no-gravity mode
    public void AttachToCrateSensor(CrateSensorController crateSensor)
    {
        if (isForcefullyAttachedToSensor) return;
        if (isBeingCarried) return;

        isForcefullyAttachedToSensor = true;

        transform.parent = crateSensor.transform;
        transform.localPosition = new Vector3(0, 0.31f, 0);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        Destroy(selfRigidbody);

        crateSensor.ActivateLogic();
    }

    public void AttachToPlayerShip(PlayerShipController playerShip)
    {
        isBeingCarried = true;
        playerShip.carriedCrate = this;
        carrierShip = playerShip;

        if (Rope_Enable)
        {
            currentRopeId = RopeManager.CreateRope(playerShip.selfRigidbody, selfRigidbody, Rope_Hinge);
        }
        else
        {
            transform.parent = playerShip.transform;
            transform.localPosition = new Vector3(0, -0.323f, 0);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            FixedJoint2D newFixedJoint = gameObject.AddComponent<FixedJoint2D>();
            newFixedJoint.connectedBody = playerShip.selfRigidbody;
        }
    }

    public void DetachFromPlayerShip(PlayerShipController playerShip, Vector2 dropoffLocation, Vector2 dropoffVelocity)
    {
        isBeingCarried = false;
        if (selfRigidbody == null) return;

        carrierShip = null;
        playerShip.carriedCrate = null;
        transform.parent = null;
        selfRigidbody.bodyType = RigidbodyType2D.Dynamic;
        Destroy(GetComponent<FixedJoint2D>());

        selfRigidbody.position = dropoffLocation;
        selfRigidbody.velocity = dropoffVelocity;

        RopeManager.DeleteRopeById(currentRopeId);
        //TODO: replace this by deleting existing rope
    }

    private float GetCurrentAngle()
    {
        float currentObjectAngle = transform.eulerAngles.z;
        currentObjectAngle %= 360; //module to 360 angles
        if (currentObjectAngle > 180) currentObjectAngle -= 360; //fix angle being over 180
        return currentObjectAngle;
    }

    private void Update()
    {
        if (ghostReplayId != -1)
        {
            if (!isBeingCarried || (isBeingCarried && (carrierShip.networkObject == null || carrierShip.networkObject.IsMine())))
            {
                GhostReplayRecorder.AddNonPlayerSnapshot(new GhostReplayNonPlayerSnapshot(ghostReplayId, transform.position.x, transform.position.y, GetCurrentAngle(), "Crate/Crate Sprite"));
            }
        }

        PlayerShipController ship = null;

        if (LevelLoader.IsPlayingSharedScreenCoop(false))
        {
            if (Input.GetKeyDown(KeysManager.GetKey(KeysManager.Player.Normal, KeysManager.Key.Crate)))
            {
                if (PlayerShipController.Ships.Count >= 1) ship = PlayerShipController.Ships[0];
            }

            if (Input.GetKeyDown(KeysManager.GetKey(KeysManager.Player.Splitscreen, KeysManager.Key.Crate)))
            {
                if (PlayerShipController.Ships.Count >= 2) ship = PlayerShipController.Ships[1];
            }
        }
        else
        {
            if (Input.GetKeyDown(KeysManager.GetKey(KeysManager.Player.Normal, KeysManager.Key.Crate)))
            {
                ship = PlayerShipController.Singletron;
            }
        }

        if (ship != null && selfRigidbody != null)
        {
            if (Time.time - ship.lastInteractedWithCrates > 0.1f)
            {
                if (ship.carriedCrate == this)
                {
                    if (NetworkingManager.CurrentLobbyValid)
                    {
                        NetworkingManager.SendPacket(new object[] { 7, crateId, false, selfRigidbody.position.x, selfRigidbody.position.y, selfRigidbody.velocity.x, selfRigidbody.velocity.y }, 1);
                    }
                    else
                    {
                        DetachFromPlayerShip(ship, selfRigidbody.position, selfRigidbody.velocity);
                    }

                    ship.lastInteractedWithCrates = Time.time;
                }
                else
                {
                    if (Vector2.Distance(transform.position, ship.transform.position) < Constants.DistanceNeededToPickupCrate && ship.carriedCrate == null)
                    {
                        AttachToPlayerShip(ship);

                        if (NetworkingManager.CurrentLobbyValid)
                        {
                            NetworkingManager.SendPacketOtherOnly(new object[] { 7, crateId, true }, 1);
                        }

                        ship.lastInteractedWithCrates = Time.time;
                    }
                }
            }
        }
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude > 4)
        {
            Destroy(gameObject);
            ExplosionsController.CreateExplosion(0, transform.position);
        }
    }*/
}
