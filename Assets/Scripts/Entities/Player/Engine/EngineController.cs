using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EngineController : ShipComponentController
{

    public enum ThrustDirection { None, UpOnly, LeftOnly, RightOnly, Any, MainMenuAuto }
    [Header("The direction in which we can push")]
    public ThrustDirection thrustDirection = ThrustDirection.Any;
    /// <summary>
    /// If this is false, we are obviously on the right side
    /// </summary>
    [Header("If thrustDirection is set to 'Any', we can choose which side this engine is on")]
    public bool isOnLeftSide;
    [Header("The thrust force")]
    public float thrustForce;
    [Header("When thrustDirection is set to 'Any' and we are turning, how much of the normal thrust force can we use to turn the ship")]
    public float turnThrustRatio = 0.8f;
    [Header("The sprite the engine will change to upon when it breaks")]
    public Sprite brokenSprite;
    [Header("The fixed joint of the leg closest to this engine")]
    public bool checkForMissingLegJoint;
    public FixedJoint2D legFixedJoint;
    [Header("The prefab for the ground dust effect")]
    public GroundDustController groundDustPrefab;
    [Header("The engine sound soruce")]
    public AudioSource engineSound;

    [HideInInspector]
    public bool brokenEngineWorking = true;

    private float nextBrokenEngineToggle;

    private SpriteRenderer spriteRenderer;
    [HideInInspector]
    public ParticleSystem.EmissionModule particlesEmissions;
    private bool overrideParticlesEmission;
    private FixedJoint2D fixedJoint;
    private bool lastEmissionsEnabled = false;
    private PlayerShipController parentShipController;

    private Rigidbody2D parentRigidbody;
    private LayerMask levelLayerMask;

    private NetworkObject parentNetworkObject;
    private ComponentHealth lastComponentHealth = ComponentHealth.Intact;

    private bool isDamaged = false;

    private bool sharedScreenCoop = false;
    private int sharedScreenCoopIndex;

    private float lastCheckedLegsGone;
    private bool isPressingLeft;
    private bool isPressingRight;
    private bool isPressingVertical;
    private float nextDustSpawnTime;

    private const float GroundDustSpawnInterval = 0.04f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        particlesEmissions = GetComponentInChildren<ParticleSystem>().emission;
        parentRigidbody = transform.parent.parent.GetComponent<Rigidbody2D>();
        fixedJoint = GetComponent<FixedJoint2D>();
        parentShipController = transform.parent.parent.GetComponent<PlayerShipController>();
        levelLayerMask = LayerMask.GetMask("Level", "Entity");

        parentNetworkObject = transform.parent.parent.gameObject.GetNetworkComponent();

        float randomRange = 0.65f;
        engineSound.pitch = Random.Range(1 - randomRange, 1 + randomRange);
    }

    private void Start()
    {
        fixedJoint = GetComponent<FixedJoint2D>();

        if (NetworkingManager.CurrentLobbyValid && parentNetworkObject != null && !parentNetworkObject.IsMine())
        {
            fixedJoint.breakForce = 9000000;
        }

        sharedScreenCoop = LevelLoader.IsPlayingSharedScreenCoop(false);
        if (sharedScreenCoop)
        {
            sharedScreenCoopIndex = PlayerShipController.Ships.IndexOf(parentShipController);
        }
    }

    /*private void Update()
    {
        if(componentHealth == ComponentHealth.Destroyed && Time.time - lastCheckedLegsGone > 1)
        {
            if (fixedJoint != null) Destroy(fixedJoint);
        }
    }*/

    private void Update()
    {
        CacheInputState();

        if (particlesEmissions.enabled)
        {
            TrySpawnGroundDust();
        }
    }

    public void DoDamagedEffect()
    {
        isDamaged = true;
        spriteRenderer.sprite = brokenSprite;

        gameObject.AddComponent(typeof(SpriteRedFlasher));
    }

    public void DoDestroyedEffect()
    {
        if (gameObject == null) return;

        componentHealth = ComponentHealth.Destroyed;

        engineSound.Stop();
        particlesEmissions.enabled = false;

        if (fixedJoint != null) Destroy(fixedJoint);
        ExplosionsController.CreateExplosion(1, transform.position);
        gameObject.AddComponent(typeof(SpriteRedFlasher));
    }

    public override ComponentHealth GetHealth()
    {
        if (fixedJoint == null)
        { //if our fixed joint is missing we are destroyed
            return ComponentHealth.Destroyed;
        }
        else if (isDamaged)
        { //if we are destroyed, well, then we are damaged
            return ComponentHealth.Broken;
        }
        else
        { //otherwise we are intact
            return ComponentHealth.Intact;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (parentNetworkObject != null && !parentNetworkObject.IsMine()) return;

        if (collision.relativeVelocity.magnitude > 1.5f)
        {
            DoDamagedEffect();

            if (isOnLeftSide)
            {
                SpaceshipHealthStatusController.UpdateShipLeftEngineHealth(GetHealth());
            }
            else
            {
                SpaceshipHealthStatusController.UpdateShipRightEngineHealth(GetHealth());
            }
        }
    }

    private void SendComponentDamagePacket(bool isDamaged)
    {
        if (parentNetworkObject != null && parentNetworkObject.IsMine())
        {
            int componentId = 1;

            if (isOnLeftSide)
            {
                componentId = 2;
            }
            else
            {
                componentId = 3;
            }

            NetworkingManager.SendPacket(new object[] { 4, componentId, isDamaged }, 1);
        }
    }

    private bool AreGameMenusOpen()
    {
        if (VictoryMenuController.Singletron != null && VictoryMenuController.Singletron.isOpen) return true;
        if (DeathMenuController.Singletron != null && DeathMenuController.Singletron.isOpen) return true;
        if (EscapeMenuController.Singletron != null && EscapeMenuController.Singletron.isOpen) return true;
        if (SourceConsole.UI.ConsoleCanvasController.IsVisible()) return true;

        return false;
    }

    private bool IsLocallyControlled()
    {
        return parentNetworkObject == null || parentNetworkObject.IsMine();
    }

    private bool ShouldTrackComponentState()
    {
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            return false;
        }

        return !NetworkingManager.CurrentLobbyValid || (parentNetworkObject != null && parentNetworkObject.IsMine());
    }

    private void CacheInputState()
    {
        isPressingLeft = Input.GetKey(KeysManager.GetKey(KeysManager.Player.Normal, KeysManager.Key.LeftEngine));
        isPressingRight = Input.GetKey(KeysManager.GetKey(KeysManager.Player.Normal, KeysManager.Key.RightEngine));
        isPressingVertical = Input.GetButton("Vertical");

        if (sharedScreenCoop && sharedScreenCoopIndex != 0)
        {
            isPressingLeft = Input.GetKey(KeysManager.GetKey(KeysManager.Player.Splitscreen, KeysManager.Key.LeftEngine));
            isPressingRight = Input.GetKey(KeysManager.GetKey(KeysManager.Player.Splitscreen, KeysManager.Key.RightEngine));
        }
    }

    private void TrySpawnGroundDust()
    {
        if (Time.time < nextDustSpawnTime)
        {
            return;
        }

        nextDustSpawnTime = Time.time + GroundDustSpawnInterval;

        float maxDistance = 1.5f;
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, -transform.up, maxDistance, levelLayerMask);

        if (raycast.collider == null)
        {
            return;
        }

        float distanceRatio = Vector2.Distance(transform.position, raycast.point) / maxDistance + Random.Range(-0.2f, 0.2f);
        float dustLifetime = Mathf.Lerp(0.75f, 0, distanceRatio);

        if (dustLifetime == 0f)
        {
            return;
        }

        GroundDustController spawnedDust = Instantiate(groundDustPrefab, (Vector3)raycast.point + new Vector3(0, 0, 1), Quaternion.LookRotation(new Vector2(0, raycast.normal.y)));
        spawnedDust.SetLifetime(dustLifetime);
        spawnedDust.SetStartAlpha(Mathf.Lerp(148, 0, distanceRatio));
    }

    private void FixedUpdate()
    {
        bool enableParticleEmissions = overrideParticlesEmission; //enable particles emission if we got a network packet saying specifically this engine is on
        bool isLocallyControlled = IsLocallyControlled();
        ComponentHealth currentHealth = GetHealth();

        if (ShouldTrackComponentState())
        {
            if (lastComponentHealth != currentHealth && currentHealth != ComponentHealth.Intact)
            {
                SendComponentDamagePacket(currentHealth == ComponentHealth.Broken);
                SteamCustomUtils.AddStat("COMPONENTS_DAMAGED");
            }

            lastComponentHealth = currentHealth;
        }

        if (!parentShipController.hasExploded && currentHealth != ComponentHealth.Destroyed)
        {
            if (currentHealth == ComponentHealth.Broken)
            {
                if (Time.time > nextBrokenEngineToggle)
                {
                    if (brokenEngineWorking)
                    {
                        nextBrokenEngineToggle = Time.time + Random.Range(0.05f, 0.15f);
                    }
                    else
                    {
                        nextBrokenEngineToggle = Time.time + Random.Range(0.4f, 0.9f);
                    }

                    brokenEngineWorking = !brokenEngineWorking;
                }

                if (!brokenEngineWorking)
                {
                    enableParticleEmissions = false;
                }
            }

            if (!AreGameMenusOpen() && isLocallyControlled && (currentHealth != ComponentHealth.Broken || brokenEngineWorking))
            {
                float thrustModifier = 1f;
                if (checkForMissingLegJoint && legFixedJoint == null)
                {
                    thrustModifier = 0.5f;
                }

                if (thrustDirection == ThrustDirection.UpOnly)
                {
                    if (isPressingVertical)
                    {
                        parentRigidbody.AddForce(transform.up * thrustForce * thrustModifier);
                        enableParticleEmissions = true;
                    }
                }
                else if (thrustDirection == ThrustDirection.LeftOnly)
                {
                    if (isPressingLeft)
                    {
                        parentRigidbody.AddForce(-transform.right * thrustForce * thrustModifier);
                        enableParticleEmissions = true;
                    }
                }
                else if (thrustDirection == ThrustDirection.RightOnly)
                {
                    if (isPressingRight)
                    {
                        parentRigidbody.AddForce(transform.right * thrustForce * thrustModifier);
                        enableParticleEmissions = true;
                    }
                }
                else if (thrustDirection == ThrustDirection.Any)
                {
                    bool bothPressed = isPressingLeft && isPressingRight;

                    if (isPressingLeft && isOnLeftSide && !bothPressed)
                    {
                        parentRigidbody.AddForceAtPosition(transform.up * thrustForce * turnThrustRatio * thrustModifier, transform.position);
                        enableParticleEmissions = true;
                    }
                    else if (isPressingRight && !isOnLeftSide && !bothPressed)
                    {
                        parentRigidbody.AddForceAtPosition(transform.up * thrustForce * turnThrustRatio * thrustModifier, transform.position);
                        enableParticleEmissions = true;
                    }
                    else if (bothPressed)
                    {
                        parentRigidbody.AddForceAtPosition(transform.up * thrustForce * thrustModifier, transform.position);
                        enableParticleEmissions = true;
                    }
                }
                else if (thrustDirection == ThrustDirection.MainMenuAuto)
                {
                    enableParticleEmissions = parentRigidbody.position.y < 5.5f && parentRigidbody.linearVelocity.magnitude > 0.6f;
                    if (enableParticleEmissions)
                    {
                        parentRigidbody.AddForce(transform.up * thrustForce * thrustModifier);
                    }
                }

                if (enableParticleEmissions && PlayerShipController.Singletron != null)
                {
                    PlayerShipController.Singletron.lastTurnedOnEngines = Time.time;
                }
            }
        }
        else
        {
            enableParticleEmissions = false;
        }

        if (currentHealth == ComponentHealth.Destroyed)
        {
            enableParticleEmissions = false;
        }

        if (isLocallyControlled && lastEmissionsEnabled != enableParticleEmissions)
        {
            NetworkingManager.SendPacketOtherOnly(new object[] { 6, isOnLeftSide ? 0 : 1, enableParticleEmissions }, 1, EP2PSend.k_EP2PSendReliable);
        }

        lastEmissionsEnabled = enableParticleEmissions;

        if (enableParticleEmissions)
        {
            if (!engineSound.isPlaying)
            {
                engineSound.Play();
            }
        }
        else
        {
            engineSound.Stop();
        }

        particlesEmissions.enabled = enableParticleEmissions;
    }

    public void OverrideParticlesEmission(bool status)
    {
        overrideParticlesEmission = status;
    }
}
