using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System;
using System.Text;

public class NetworkObject : MonoBehaviour {
    public struct State {
        public double time;
        public Vector2 pos;
        public float angle;
    }

    public int id;
    public ulong owner;

    [HideInInspector]
    public bool enableSending = true;

    [HideInInspector]
    public Rigidbody2D selfRigidbody;
    [HideInInspector]
    public State[] bufferedState = new State[10];

    private int timeCount;
    private float interpolationDelay = 0.15f;

    private float updateIntervals = 1f / 60f;
    private float lastSentUpdate;

    private void Awake() {
        if (SteamManager.Initialized && !NetworkingManager.CurrentLobbyValid) { //if we are not in a lobby
            Destroy(this); //destroy this component
            return;
        }

        selfRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        if (selfRigidbody != null) {
            if (IsMine()) {
                selfRigidbody.bodyType = RigidbodyType2D.Dynamic;
            } else {
                selfRigidbody.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    private void FixedUpdate() {
        if (!NetworkingManager.CurrentLobbyValid) return;

        if(enableSending && IsMine() && Time.time > lastSentUpdate + updateIntervals) {
            lastSentUpdate = Time.time;

            List<object> sendData = new List<object>();
            sendData.Add(3);
            sendData.Add(id);
            if(selfRigidbody != null) {
                sendData.Add(selfRigidbody.position.x);
                sendData.Add(selfRigidbody.position.y);
            } else {
                sendData.Add(transform.position.x);
                sendData.Add(transform.position.y);
            }

            sendData.Add(GetCurrentAngle());

            NetworkingManager.SendPacketOtherOnly(sendData.ToArray(), 1, EP2PSend.k_EP2PSendUnreliableNoDelay); //if this lags, use unreliable
        }
    }

    public float GetCurrentAngle() {
        float currentObjectAngle = transform.eulerAngles.z;
        currentObjectAngle %= 360; //module to 360 angles
        if (currentObjectAngle > 180) currentObjectAngle -= 360; //fix angle being over 180
        return currentObjectAngle;
    }

    private void Update() {
        if (IsMine() || !NetworkingManager.CurrentLobbyValid) return;

        double currentTime = Time.time;
        double interpolationTime = currentTime - interpolationDelay;

        if(bufferedState[0].time > interpolationTime) {
            for(int i = 0; i < timeCount; i++) {
                if (this.bufferedState[i].time <= interpolationTime || i == this.timeCount - 1) {
                    // The state one slot newer (<100ms) than the best playback state
                    State rhs = this.bufferedState[Mathf.Max(i - 1, 0)];
                    // The best playback state (closest to 100 ms old (default time))
                    State lhs = this.bufferedState[i];

                    // Use the time between the two slots to determine if interpolation is necessary
                    double diffBetweenUpdates = rhs.time - lhs.time;
                    float t = 0.0F;
                    // As the time difference gets closer to 100 ms t gets closer to 1 in 
                    // which case rhs is only used
                    if (diffBetweenUpdates > 0.0001) {
                        t = (float)((interpolationTime - lhs.time) / diffBetweenUpdates);
                    }

                    // if t=0 => lhs is used directly
                    if(selfRigidbody != null) {
                        selfRigidbody.position = Vector2.Lerp(lhs.pos, rhs.pos, t);
                        selfRigidbody.rotation = Mathf.LerpAngle(lhs.angle, rhs.angle, t);
                    } else {
                        transform.position = Vector2.Lerp(lhs.pos, rhs.pos, t);
                        transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(lhs.angle, rhs.angle, t));
                    }
                    return;
                }
            }
        } else {
            State latest = bufferedState[0];

            if (selfRigidbody != null) {
                selfRigidbody.position = Vector2.Lerp(transform.position, latest.pos, Time.deltaTime * 20);
                selfRigidbody.rotation = latest.angle;
            } else {
                transform.position = Vector2.Lerp(transform.position, latest.pos, Time.deltaTime * 20);
                transform.eulerAngles = new Vector3(0, 0, latest.angle);
            }
        }
    }

    public void AddState(State newState) {
        //Shift buffer contents, oldest data erased, 18 becomes 19, ... , 0 becomes 1
        for (int i = bufferedState.Length - 1; i >= 1; i--) {
            bufferedState[i] = bufferedState[i - 1];
        }

        bufferedState[0] = newState;

        timeCount = Mathf.Min(timeCount + 1, bufferedState.Length);

        // Check integrity, lowest numbered state in the buffer is newest and so on
        for (int i = 0; i < this.timeCount - 1; i++) {
            if (this.bufferedState[i].time < this.bufferedState[i + 1].time) {
                Debug.Log("State inconsistent");
            }
        }
    }

    /// <summary>
    /// Is this object controlled/owned by me
    /// </summary>
    /// <returns></returns>
    public bool IsMine() {
        if (!SteamManager.Initialized) return true;

        return owner == (ulong) SteamUser.GetSteamID();
    }

    private void OnDisable() {
        if (NetworkingManager.CurrentLobbyValid) {
            NetworkingManager.SendPacket(new object[] { 2, id }, 1, EP2PSend.k_EP2PSendReliable);
        }
    }
}
