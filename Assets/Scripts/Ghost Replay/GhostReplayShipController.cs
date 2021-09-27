using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

class GhostReplayShipController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerNameText;

    [Header("Body Skin Sprite")]
    public SpriteRenderer mainBodyRenderer;

    [Header("Engine Sprites")]
    [SerializeField]
    private SpriteRenderer leftEngine;
    [SerializeField]
    private SpriteRenderer rightEngine;

    [Header("Leg Sprites")]
    [SerializeField]
    private SpriteRenderer leftLegTop;
    [SerializeField]
    private SpriteRenderer leftLegBottom;
    [SerializeField]
    private SpriteRenderer rightLegTop;
    [SerializeField]
    private SpriteRenderer rightLegBottom;

    [Header("Engine particles sprite")]
    [SerializeField]
    private SpriteRenderer leftEngineParticles;
    [SerializeField]
    private SpriteRenderer rightEngineParticles;

    [Header("Sprites")]
    [SerializeField]
    private Sprite intactEngine;
    [SerializeField]
    private Sprite brokenEngine;

    private void SetRecursiveLayer(Transform obj, int layer)
    {
        obj.gameObject.layer = LayerMask.NameToLayer("Death Ghost Replay");
        foreach (Transform child in obj)
        {
            SetRecursiveLayer(child, layer);
        }
    }

    private void Awake()
    {
        if (!GhostReplay.Enabled)
        {
            SetRecursiveLayer(transform, 12);
        }
    }

    public void Setup(int shipIndex, string playerName, GhostReplaySnapshot firstSnapshot, bool switchToGhostReplayLayer = false)
    {
        if (firstSnapshot == null || firstSnapshot.playerSnapshots == null) return;

        mainBodyRenderer.sprite = ShipSkinsManager.Skins[shipIndex].GetSprite();
        playerNameText.text = playerName;

        if (firstSnapshot.playerSnapshots.ContainsKey(shipIndex))
        {
            UpdateComponentsHealth(firstSnapshot.playerSnapshots[shipIndex]);
        }
        
        if (switchToGhostReplayLayer)
        {
            SetRecursiveLayer(transform, 12);
        }
    }

    public void UpdateComponentsHealth(GhostReplayPlayerSnapshot currentSnapshot)
    {
        var leftEngineHealth = currentSnapshot.leftEngine;
        var rightEngineHealth = currentSnapshot.rightLeg;
        var leftLegHealth = currentSnapshot.leftLeg;
        var rightLegHealth = currentSnapshot.rightLeg;

        //left engine
        if (leftEngineHealth == ShipComponentController.ComponentHealth.Destroyed)
        {
            leftEngine.color = GhostReplayPlayback.LerpAlphaToZero(leftEngine.color);
        }
        else
        {
            leftEngine.color = GhostReplayPlayback.LerpAlphaToFull(leftEngine.color);
        }
        if (leftEngineHealth != ShipComponentController.ComponentHealth.Intact)
        {
            leftEngine.sprite = brokenEngine;
        }
        else
        {
            leftEngine.sprite = intactEngine;
        }

        //right engine
        if (rightEngineHealth == ShipComponentController.ComponentHealth.Destroyed)
        {
            rightEngine.color = GhostReplayPlayback.LerpAlphaToZero(rightEngine.color);
        }
        else
        {
            rightEngine.color = GhostReplayPlayback.LerpAlphaToFull(rightEngine.color);
        }
        if (rightEngineHealth != ShipComponentController.ComponentHealth.Intact)
        {
            rightEngine.sprite = brokenEngine;
        }
        else
        {
            rightEngine.sprite = intactEngine;
        }

        //left leg
        if (leftLegHealth == ShipComponentController.ComponentHealth.Intact)
        {
            leftLegTop.color = GhostReplayPlayback.LerpAlphaToFull(leftLegTop.color);
            leftLegBottom.color = GhostReplayPlayback.LerpAlphaToFull(leftLegBottom.color);
        }
        else if (leftLegHealth == ShipComponentController.ComponentHealth.Broken)
        {
            leftLegTop.color = GhostReplayPlayback.LerpAlphaToFull(leftLegTop.color);
            leftLegBottom.color = GhostReplayPlayback.LerpAlphaToZero(leftLegBottom.color);
        }
        else
        {
            leftLegTop.color = GhostReplayPlayback.LerpAlphaToZero(leftLegTop.color);
            leftLegBottom.color = GhostReplayPlayback.LerpAlphaToZero(leftLegBottom.color);
        }

        //right leg
        if (rightLegHealth == ShipComponentController.ComponentHealth.Intact)
        {
            rightLegTop.color = GhostReplayPlayback.LerpAlphaToFull(rightLegTop.color);
            rightLegBottom.color = GhostReplayPlayback.LerpAlphaToFull(rightLegBottom.color);
        }
        else if (rightLegHealth == ShipComponentController.ComponentHealth.Broken)
        {
            rightLegTop.color = GhostReplayPlayback.LerpAlphaToFull(rightLegTop.color);
            rightLegBottom.color = GhostReplayPlayback.LerpAlphaToZero(rightLegBottom.color);
        }
        else
        {
            rightLegTop.color = GhostReplayPlayback.LerpAlphaToZero(rightLegTop.color);
            rightLegBottom.color = GhostReplayPlayback.LerpAlphaToZero(rightLegBottom.color);
        }
    }

    public void UpdatePerSnapshot(float lerp, GhostReplayPlayerSnapshot currentSnapshot, GhostReplayPlayerSnapshot nextSnapshot)
    {
        if (currentSnapshot == null) return; //backwards compability

        transform.position = Vector2.Lerp(currentSnapshot.GetPosition(), nextSnapshot.GetPosition(), lerp);
        transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(currentSnapshot.rotation, nextSnapshot.rotation, lerp));

        //setting alphas and stuff
        mainBodyRenderer.color = GhostReplayPlayback.LerpAlphaToFull(mainBodyRenderer.color);

        //engine particles
        leftEngineParticles.color = GhostReplayPlayback.LerpAlphaToFull(leftEngineParticles.color);
        leftEngineParticles.enabled = currentSnapshot.leftEngineOn;

        rightEngineParticles.color = GhostReplayPlayback.LerpAlphaToFull(rightEngineParticles.color);
        rightEngineParticles.enabled = currentSnapshot.rightEngineOn;

        UpdateComponentsHealth(currentSnapshot);
    }

    public void Hide(float lerp)
    {
        //setting all alphas to zero
        mainBodyRenderer.color = GhostReplayPlayback.LerpAlphaToZero(mainBodyRenderer.color, lerp);

        leftEngine.color = GhostReplayPlayback.LerpAlphaToZero(leftEngine.color, lerp);
        rightEngine.color = GhostReplayPlayback.LerpAlphaToZero(rightEngine.color, lerp);

        leftLegTop.color = GhostReplayPlayback.LerpAlphaToZero(leftLegTop.color, lerp);
        leftLegBottom.color = GhostReplayPlayback.LerpAlphaToZero(leftLegBottom.color, lerp);
        rightLegTop.color = GhostReplayPlayback.LerpAlphaToZero(rightLegTop.color, lerp);
        rightLegBottom.color = GhostReplayPlayback.LerpAlphaToZero(rightLegBottom.color, lerp);

        leftEngineParticles.color = GhostReplayPlayback.LerpAlphaToZero(leftEngineParticles.color, lerp);
        rightEngineParticles.color = GhostReplayPlayback.LerpAlphaToZero(rightEngineParticles.color, lerp);

        playerNameText.text = "";
    }

    public void HideImmediately()
    {
        Hide(1);
    }

    public float GetBodyRendererAlpha()
    {
        return mainBodyRenderer.color.a;
    }
}
