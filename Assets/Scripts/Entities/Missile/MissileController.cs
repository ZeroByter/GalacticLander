using UnityEngine;

public class MissileController : MonoBehaviour
{
    public float maxEngineVolume = 1;

    [HideInInspector]
    public MissileLauncherController missileLauncher;

    private float missileLaunchTime = 0.3f; //the time the missile has to get away from it's launcher before becoming operational
    private float missileLaunchSpeed = 0.065f; //the missile launch speed
    private float missileSpeed = 0.018f; //the missile normal operational speed
    private float missileTurnSpeed = 0.05f;

    private float firedTime;
    private float timeSinceFired
    {
        get
        {
            return Time.time - firedTime;
        }
    }

    private PlayerShipController closestShip;
    private ParticleSystem engineParticles;
    private ParticleSystem.EmissionModule engineParticlesEmssion;
    private AudioSource soundSource;

    private NetworkObject nObject;

    private int ghostReplayId = -1;

    private int layersMask;
    private int levelLayerMask;

    private MissileLauncherController GetClosestMissileLauncher()
    {
        MissileLauncherController closest = null;

        foreach (GameObject entityObject in GameObject.FindGameObjectsWithTag("Entity"))
        { //loop through all entities
            MissileLauncherController controller = entityObject.GetComponent<MissileLauncherController>();

            if (controller != null)
            {
                if (closest == null || Vector2.Distance(transform.position, controller.transform.position) < Vector2.Distance(transform.position, closest.transform.position))
                {
                    closest = controller;
                }
            }
        }

        return closest;
    }

    private void Awake()
    {
        firedTime = Time.time;

        engineParticles = transform.GetChild(0).GetComponent<ParticleSystem>();
        engineParticlesEmssion = engineParticles.emission;

        soundSource = GetComponent<AudioSource>();
        soundSource.volume = 0;

        if (NetworkingManager.CurrentLobbyValid)
        { //if we are in a lobby it means our missile launcher variable is not set, we have to fix that
            MissileLauncherController closestLauncher = GetClosestMissileLauncher(); //get closest launcher

            if (closestLauncher != null)
            { //if it's valid
                //setup vars
                closestLauncher.currentlyFiredMissile = this;
                missileLauncher = closestLauncher;
            }
        }

        layersMask = LayerMask.GetMask("Player", "Level");
    }

    private void Start()
    {
        nObject = gameObject.GetNetworkComponent();

        if (GhostReplayRecorder.Singleton != null) ghostReplayId = GhostReplayRecorder.Singleton.GetCurrentGhostReplay().GetNewGhostReplayId();
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
            GhostReplayRecorder.AddNonPlayerSnapshot(new GhostReplayNonPlayerSnapshot(ghostReplayId, transform.position.x, transform.position.y, GetCurrentAngle(), "Missile/Missile"));
        }

        soundSource.volume = Mathf.Lerp(soundSource.volume, maxEngineVolume, 0.35f);

        closestShip = PlayerShipController.GetClosestShip(transform.position);

        if (closestShip != null)
        {
            Vector3 closestShipPosition = closestShip.transform.position;

            if (nObject.owner != closestShip.networkObject.owner) nObject.owner = closestShip.networkObject.owner;

            if (timeSinceFired <= missileLaunchTime)
            {
                engineParticlesEmssion.enabled = false;
                return;
            }
            else
            {
                engineParticlesEmssion.enabled = true;
            }

            Vector3 dir = closestShipPosition - transform.position;
            Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90, 90);
            if (rot.eulerAngles.y == 180)
            {
                Vector3 euler = rot.eulerAngles;
                euler.y = 0;
                euler.z = 360 - (euler.z - 180) - 180; //idk if this specific line could be optimized or not but whateves
                rot.eulerAngles = euler;
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, rot, missileTurnSpeed);
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);

            float targetDistance = Vector3.Distance(closestShipPosition, transform.position);

            var rayHit = Physics2D.Raycast(transform.position + transform.up * 0.251f, transform.up, 0.1f);
            if (rayHit)
            {
                if (rayHit.transform.gameObject.layer == 10)
                {
                    ExplodeAndKill(closestShip, closestShipPosition, targetDistance);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (NetworkingManager.CurrentLobbyValid)
        { //if we are in a lobby
            if (nObject == null || !nObject.IsMine())
            { //if we are not the player who instantiated this missile launcher (the lobby owner) then halt/exit method
                return;
            }
        }

        if (missileLauncher != null)
        {
            if (Vector2.Distance(missileLauncher.transform.position, transform.position) > Constants.MissileKillDistance)
            { //if we got out of range of our missile launcher
                Destroy(gameObject);
            }
        }

        float speed = missileSpeed;

        if (timeSinceFired <= missileLaunchTime)
        {
            speed = Mathf.Lerp(missileLaunchSpeed, missileSpeed, timeSinceFired / missileLaunchTime);
        }

        transform.position = transform.TransformPoint(Vector3.up * speed); //move forward at a certain speed
    }

    private void OnDestroy()
    {
        MainCameraController.StartShake(0.025f, 1f);
        ExplosionsController.CreateExplosion(0, transform.position);
    }

    private GameObject GetPlayerParentObject(GameObject go)
    {
        if (go.transform.parent != null && go.transform.parent.gameObject.tag == "Player")
        { //if parent of object is a player object
            return GetPlayerParentObject(go.transform.parent.gameObject);
        }
        else
        { //if parent is not parent object
            return go;
        }
    }

    private void ExplodeAndKill(PlayerShipController ship, Vector3 shipPosition, float distanceToShip)
    {
        if (NetworkingManager.CurrentLobbyValid)
        { //if we are in a lobby
            if (nObject == null || !nObject.IsMine()) //if we are not the player who instantiated this missile (the owner of the ship who was closest to the launcher) then halt/exit method
            {
                return;
            }
        }

        ExplosionsController.CreateExplosion(0, Vector2.Lerp(transform.position, shipPosition, 0.5f));

        float lerpShakeMagnitudeFactor = Mathf.Lerp(2, 0, distanceToShip / 5);
        float lerpShakeDurationFactor = Mathf.Lerp(0.5f, 0, distanceToShip / 5);
        MainCameraController.StartShake(lerpShakeMagnitudeFactor, lerpShakeDurationFactor);

        if (NetworkingManager.CurrentLobbyValid)
        {
            NetworkingManager.SendPacket(new object[] { 4, 4, true }, 1);
        }
        else
        {
            ship.NetworkComponentDamaged(4, true);
        }

        Destroy(gameObject);
    }
}
