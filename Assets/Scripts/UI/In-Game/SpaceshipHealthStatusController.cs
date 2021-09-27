using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipHealthStatusController : MonoBehaviour {
    public static SpaceshipHealthStatusController Singleton;

    public LerpCanvasGroup canvasGroup;

    [Header("The alpha used for images")]
    [Range(0, 1)]
    public float alpha;

    [Header("Lerp image colors")]
    public LerpImageColor body;
    public LerpImageColor leftEngine;
    public LerpImageColor rightEngine;
    public LerpImageColor leftLeg;
    public LerpImageColor rightLeg;

    private void Awake()
    {
        Singleton = this;
    }

    private static void ApplyCorrectColorTarget(LerpImageColor lerpImage, ShipComponentController.ComponentHealth health)
    {
        if(health == ShipComponentController.ComponentHealth.Intact)
        {
            lerpImage.target = new Color(1, 1, 1, Singleton.alpha);
        }
        else if(health == ShipComponentController.ComponentHealth.Broken)
        {
            lerpImage.target = new Color(0.8f, 0, 0, Singleton.alpha);
        }
        else
        {
            lerpImage.target = new Color(0, 0, 0, 0);
        }
    }

    public static void UpdateShipBodyHealth(ShipComponentController.ComponentHealth health)
    {
        if (Singleton == null) return;

        if(health == ShipComponentController.ComponentHealth.Destroyed)
        {
            Singleton.canvasGroup.target = 0f;
        }

        ApplyCorrectColorTarget(Singleton.body, health);
    }
    public static void UpdateShipLeftEngineHealth(ShipComponentController.ComponentHealth health)
    {
        if (Singleton == null) return;

        ApplyCorrectColorTarget(Singleton.leftEngine, health);
    }

    public static void UpdateShipRightEngineHealth(ShipComponentController.ComponentHealth health)
    {
        if (Singleton == null) return;

        ApplyCorrectColorTarget(Singleton.rightEngine, health);
    }

    public static void UpdateShipLeftLegHealth(ShipComponentController.ComponentHealth health)
    {
        if (Singleton == null) return;

        ApplyCorrectColorTarget(Singleton.leftLeg, health);
    }

    public static void UpdateShipRightLegHealth(ShipComponentController.ComponentHealth health)
    {
        if (Singleton == null) return;

        ApplyCorrectColorTarget(Singleton.rightLeg, health);
    }
}
