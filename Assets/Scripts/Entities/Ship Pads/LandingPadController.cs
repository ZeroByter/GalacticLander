using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;

public class LandingPadController : MonoBehaviour {
    public static int NextLandPadId = 0;
    public static List<LandingPadController> AllPads = new List<LandingPadController>();

    private static List<LandingPadController> CapturedPads = new List<LandingPadController>();

    public LightBulbController lightsController;

    [HideInInspector]
    public int landPadId;

    [HideInInspector]
    public bool isCapturing;

    [HideInInspector]
    public float lastStartedCapturing;
    private float lastStoppedCapturing;

    private bool isLocalPlayerCapturing;
    private float maxCapture = 4;

    private bool playCapturingSoundForAllPlayers = false;
    
    private Collider2D selfCollider;
    
    private AudioSource selfAudio;
    private bool isCoop;

    private void OnEnable() {
        AllPads.Add(this);
    }

    private void OnDestroy() {
        AllPads.Remove(this);
    }

    private void Awake() {
        landPadId = NextLandPadId;
        NextLandPadId++;

        selfAudio = GetComponent<AudioSource>();
        selfCollider = GetComponent<Collider2D>();

        isCoop = SteamManager.Initialized && NetworkingManager.CurrentLobbyValid;

        if (isCoop) selfAudio.volume = 0.4f;
    }

    private GameObject GetPlayerShipParent(GameObject gameObject) {
        if(gameObject.GetComponent<PlayerShipController>() != null) {
            return gameObject;
        } else {
            return GetPlayerShipParent(gameObject.transform.parent.gameObject);
        }
    }

    private void SendPacket(bool isCapturing) {
        NetworkingManager.SendPacket(new object[] { 11, landPadId, isCapturing, LevelLoader.GetLevelDirectory() }, 1);
    }
    
    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.gameObject.tag == "Player") {
            ShipComponentController componentController = collision.gameObject.GetComponent<ShipComponentController>();
            if(componentController != null) {
                if (componentController.GetHealth() == ShipComponentController.ComponentHealth.Destroyed) return;
            }

            if (isCoop) {
                NetworkObject nObject = GetPlayerShipParent(collision.gameObject).GetNetworkComponent();

                if(nObject.IsMine()) {
                    SendPacket(collision.transform.position.y > transform.position.y);
                }
            } else {
                if (!isCapturing) lastStartedCapturing = Time.time;
                isCapturing = collision.transform.position.y > transform.position.y;

                NetworkObject nObject = collision.gameObject.GetNetworkComponent();
                isLocalPlayerCapturing = nObject == null || (nObject != null && nObject.IsMine());
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            ShipComponentController componentController = collision.gameObject.GetComponent<ShipComponentController>();
            if (componentController != null) {
                if (componentController.GetHealth() == ShipComponentController.ComponentHealth.Destroyed) return;
            }

            if (isCoop) {
                NetworkObject nObject = GetPlayerShipParent(collision.gameObject).GetNetworkComponent();

                if (nObject.IsMine()) {
                    SendPacket(collision.transform.position.y > transform.position.y);
                }
            } else {
                isCapturing = collision.transform.position.y > transform.position.y;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Player") {
            if (isCoop) {
                NetworkObject nObject = GetPlayerShipParent(collision.gameObject).GetNetworkComponent();

                if (nObject.IsMine()) {
                    SendPacket(selfCollider.IsTouching(collision.collider));
                }
            } else {
                isCapturing = selfCollider.IsTouching(collision.collider);
            }
        }
    }

    private void Update() {
        if (isCapturing) {
            float capturingProgress = Time.time - lastStartedCapturing;

            if (PlayerShipController.Singletron.developerMode) capturingProgress = maxCapture;

            lightsController.progress = capturingProgress / maxCapture;

            if(!isCoop || (isCoop && isLocalPlayerCapturing) || playCapturingSoundForAllPlayers) {
                if (lightsController.progress < 0.95f) {
                    if (!selfAudio.isPlaying) selfAudio.Play();
                    selfAudio.pitch = Mathf.Lerp(0, 2, capturingProgress / maxCapture);
                } else {
                    if (VictoryMenuController.Singletron.isOpen) {
                        selfAudio.volume = Mathf.Lerp(selfAudio.volume, 0, 0.04f);
                    }
                }
            } else {
                selfAudio.Stop();
            }

            if (capturingProgress >= maxCapture) {
                if (!CapturedPads.Contains(this)) CapturedPads.Add(this);

                if (!isCoop && !LevelLoader.IsPlayingSharedScreenCoop(false)) {
                    if (LevelLoader.PlayTestingLevel) { //if we are playtesting this level
                        SceneManager.LoadScene("Level Editor");
                    } else {
                        VictoryMenuController.OpenMenu();
                    }
                }
            }
        } else {
            if (CapturedPads.Contains(this)) CapturedPads.Remove(this);

            if (lastStoppedCapturing < lastStartedCapturing) lastStoppedCapturing = Time.time;
            lightsController.progress = Mathf.Lerp(lightsController.progress, 0, (Time.time - lastStoppedCapturing) / (maxCapture * 2));

            if (!isCoop || (isCoop && isLocalPlayerCapturing) || playCapturingSoundForAllPlayers) {
                selfAudio.pitch = lightsController.progress;
            } else {
                selfAudio.Stop();
            }

            if (lightsController.progress <= 0) {
                lightsController.progress = 0;
                selfAudio.Stop();
            }
        }

        if(CapturedPads.Count == 2) {
            VictoryMenuController.OpenMenu();
        }
    }

    public static void ResetCapturedLandingPads() {
        CapturedPads.Clear();
    }
}
