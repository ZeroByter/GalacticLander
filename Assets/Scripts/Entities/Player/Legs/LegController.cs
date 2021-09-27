using UnityEngine;
using UnityEngine.SceneManagement;

public class LegController : ShipComponentController {
    public bool isLeftLeg;
    public FixedJoint2D topFixedJoint;
    public HingeJoint2D bottomHingeJoint;
    public FixedJoint2D bottomFixedJoint;

    private NetworkObject networkObject;

    private ComponentHealth lastComponentHealth = ComponentHealth.Intact;

    private void Awake() {
        networkObject = transform.parent.parent.gameObject.GetNetworkComponent();
    }

    private void Start() {
        if(NetworkingManager.CurrentLobbyValid && networkObject != null && !networkObject.IsMine()) {
            topFixedJoint.breakForce = 9000000;
            bottomFixedJoint.breakForce = 9000000;
        }
    }

    private void Update() {
        if (SceneManager.GetActiveScene().name == "Main Menu") return;

        if(lastComponentHealth != GetHealth()) { //the code in this if statement gets executed when the health of the component changes in any way (if it literally not what it was during the last update method call)
            if(GetHealth() != ComponentHealth.Intact) {
                transform.GetChild(0).gameObject.AddComponent(typeof(SpriteRedFlasher));
                transform.GetChild(1).gameObject.AddComponent(typeof(SpriteRedFlasher));
                ExplosionsController.CreateExplosion(1, bottomHingeJoint.transform.position);
            }

            if(GetHealth() == ComponentHealth.Destroyed) {
                transform.GetChild(0).gameObject.tag = "Untagged";
                transform.GetChild(1).gameObject.tag = "Untagged";
            }

            if(GetHealth() != ComponentHealth.Intact) {
                SteamCustomUtils.AddStat("COMPONENTS_DAMAGED");
            }

            if(GetHealth() != ComponentHealth.Intact) {
                if (networkObject != null && networkObject.IsMine())
                {
                    int componentId = 1;
                    bool isDamaged = false;

                    if (transform.localScale.x == 1)
                    { //if the scale x component equals to 1 it means we are the right-hand leg controller of the ship
                        componentId = 1;
                    }
                    else
                    {
                        componentId = 0;
                    }

                    if (GetHealth() == ComponentHealth.Broken)
                    {
                        isDamaged = true;
                    }
                    if (GetHealth() == ComponentHealth.Destroyed)
                    {
                        isDamaged = false;
                    }

                    if (GetHealth() != ComponentHealth.Intact)
                    {
                        NetworkingManager.SendPacketOtherOnly(new object[] { 4, componentId, isDamaged }, 1);
                    }
                }

                if (isLeftLeg)
                {
                    SpaceshipHealthStatusController.UpdateShipLeftLegHealth(GetHealth());
                }
                else
                {
                    SpaceshipHealthStatusController.UpdateShipRightLegHealth(GetHealth());
                }
            }
        }
        
        lastComponentHealth = GetHealth();
    }

    public override ComponentHealth GetHealth() {
        if (topFixedJoint == null) { //if the top fixed joint is broken (meaning the landing leg is just laying around somewhere) then the leg is destroyed (duh)
            return ComponentHealth.Destroyed;
        }else if(bottomFixedJoint == null) { //if the bottom fixed joint is broken and the leg is just tangling there, we are broken
            return ComponentHealth.Broken;
        } else {
            return ComponentHealth.Intact; //otherwise we are intact
        }
    }
}
