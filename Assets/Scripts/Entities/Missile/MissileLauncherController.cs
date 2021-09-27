using Steamworks;
using UnityEngine;

public class MissileLauncherController : MonoBehaviour {
    [HideInInspector]
    public MissileController currentlyFiredMissile;

    private float fireMissileCooldown;

    private void FireMissile() {
        if (Time.timeSinceLevelLoad < 1) return;

        if (NetworkingManager.CurrentLobbyValid) { //if we are in coop
            if(SteamMatchmaking.GetLobbyOwner((CSteamID) NetworkingManager.CurrentLobby) == SteamUser.GetSteamID()) { //if we are the lobby owner
                float angle = transform.eulerAngles.z;
                angle %= 360;
                if (angle > 180) angle -= 360;

                NetworkingManager.InstantiateObject("Missile/Missile", transform.position, angle);

                fireMissileCooldown = Time.time;
            }
        } else {
            MissileController newMissile = Instantiate(Resources.Load<GameObject>("Missile/Missile")).GetComponent<MissileController>();
            currentlyFiredMissile = newMissile;
            newMissile.transform.position = transform.position;
            newMissile.transform.rotation = transform.rotation;
            newMissile.gameObject.SetActive(true);
            newMissile.missileLauncher = this;
        }
    }

    private void Update() {
        if (currentlyFiredMissile != null) fireMissileCooldown = Time.time;

        PlayerShipController closestShip = PlayerShipController.GetClosestShip(transform.position);

        if(closestShip != null) {
            Vector2 directionToTarget = (closestShip.transform.position - transform.position).normalized;
            float dot = Vector2.Dot(directionToTarget, transform.up);

            //if ship target is infront of us, close enough, and we are not being cooled down than we fire missile!
            if (dot > 0.7 && Vector2.Distance(closestShip.transform.position, transform.position) < Constants.MissileLauncherTriggerDistance && Time.time > fireMissileCooldown + Constants.MissileLauncherLaunchCooldown) {
                FireMissile();
            }
        }
    }
}
