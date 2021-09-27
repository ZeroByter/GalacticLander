using UnityEngine;

public class FirstMovementTimer : MonoBehaviour {
    private static float FirstMovementTime = 0;

    public static float GetFirstMovementTime() {
        return FirstMovementTime;
    }

    public static float GetTimeSinceFirstMovement() {
        return Time.time - FirstMovementTime;
    }

    private void Awake() {
        FirstMovementTime = 0;
    }

    private void Update() {
        bool isPressingMovementButtons = Input.GetButtonDown("Left") || Input.GetButtonDown("Right") || Input.GetButtonDown("Vertical");

        if (isPressingMovementButtons && FirstMovementTime == 0 && !SourceConsole.UI.ConsoleCanvasController.IsVisible()) {
            FirstMovementTime = Time.time;
            SourceConsole.SourceConsole.print("Set first movement time as " + FirstMovementTime);
        }
    }
}


//this is the code for if we want to try to sync the finish times between clients
/*public class FirstMovementTimer : MonoBehaviour {
    private static bool isCoop {
        get {
            return SteamManager.Initialized && NetworkingManager.CurrentLobbyValid;
        }
    }

    private static float _FirstMovementTime = 0;
    private static float FirstMovementTime {
        get {
            if (isCoop) {
                string firstMovementTime = SteamMatchmaking.GetLobbyData((CSteamID)NetworkingManager.CurrentLobby, "firstMovement");
                if (string.IsNullOrEmpty(firstMovementTime)) {
                    return 0;
                } else {
                    return float.Parse(SteamMatchmaking.GetLobbyData((CSteamID)NetworkingManager.CurrentLobby, "firstMovement"));
                }
            } else {
                return _FirstMovementTime;
            }
        }
        set {
            if (isCoop) {
                SteamMatchmaking.SetLobbyData((CSteamID)NetworkingManager.CurrentLobby, "firstMovement", value.ToString());
            } else {
                _FirstMovementTime = value;
            }
        }
    }
    
    public static float GetTimeSinceFirstMovement() {
        if (FirstMovementTime == 0) return 0;

        if (isCoop) {
            return SteamUtils.GetServerRealTime() - FirstMovementTime;
        } else {
            return Time.time - FirstMovementTime;
        }
    }

    private void Awake() {
        FirstMovementTime = 0;
    }

    private void Update() {
        bool isPressingMovementButtons = Input.GetButtonDown("Left") || Input.GetButtonDown("Right") || Input.GetButtonDown("Vertical");

        if (isPressingMovementButtons && FirstMovementTime == 0) {
            if (isCoop) {
                FirstMovementTime = SteamUtils.GetServerRealTime();
            } else {
                FirstMovementTime = Time.time;
            }
            print("set first movement time as " + FirstMovementTime + " - are we in coop? = " + isCoop);
        }
    }
}*/
