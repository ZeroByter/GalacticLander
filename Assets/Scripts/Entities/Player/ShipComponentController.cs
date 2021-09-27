using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipComponentController : MonoBehaviour {
    [SerializeField]
    public enum ComponentHealth { Intact, Broken, Destroyed }
    [HideInInspector]
    public ComponentHealth componentHealth = ComponentHealth.Intact;

    public virtual ComponentHealth GetHealth() {
        return componentHealth;
    }

    public void ComponentBroken() {
        MainCameraController.StartShake(0.25f, 0.3f);
    }

    public void ComponentDestroyed() {
        MainCameraController.StartShake(0.65f, 0.6f);
    }
}
