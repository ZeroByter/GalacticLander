using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CrateSensorController : MonoBehaviour {
    public static List<CrateSensorController> CrateSensors = new List<CrateSensorController>();
    public static int NextCrateSensorId;

    public static CrateSensorController GetCrateSensorById(int id)
    {
        foreach (CrateSensorController crateSensor in CrateSensors)
        {
            if (crateSensor.crateSensorId == id) return crateSensor;
        }

        return null;
    }

    [HideInInspector]
    public int crateSensorId;

    public TMP_Text text;

    private LevelObjectHolder objectHolder;

    private AudioSource selfAudio;

    private void Awake() {
        selfAudio = GetComponent<AudioSource>();
    }

    private void Start() {
        objectHolder = GetComponent<LevelObjectHolder>();
        text.text = "SENSOR #" + objectHolder.levelEntity.logicNumber;
    }

    private void OnEnable()
    {
        if (CrateSensors == null) CrateSensors = new List<CrateSensorController>();

        CrateSensors.Add(this);

        crateSensorId = NextCrateSensorId;
        NextCrateSensorId++;
    }

    private void OnDisable()
    {
        CrateSensors.Remove(this);
    }

    private void Update()
    {
        if (LevelLoader.GravityEnabled) return;

        foreach(var crate in CrateController.Crates)
        {
            if(!crate.GetIsBeingCarried() && Vector2.Distance(crate.transform.position, transform.position) < 1.35f)
            {
                crate.AttachToCrateSensor(this);
                NetworkingManager.SendPacketOtherOnly(new object[] { 13, crateSensorId, crate.crateId });
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if(collision.gameObject.tag == "Crate") {
            Vector2 localPosition = transform.InverseTransformPoint(collision.transform.position);

            if(localPosition.x > -0.157f && localPosition.x < 0.173f && localPosition.y > 0.05f && localPosition.y < 0.6f) {
                ActivateLogic();
            } else {
                if (objectHolder != null) objectHolder.levelEntity.DeactivateLogic();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (objectHolder != null) objectHolder.levelEntity.DeactivateLogic();
    }

    public void ActivateLogic()
    {
        if (objectHolder != null)
        {
            if (!objectHolder.levelEntity.isLogicActivated && objectHolder.levelEntity.logicTarget != null)
            {
                selfAudio.Play();
                objectHolder.levelEntity.ActivateLogic();
            }
        }
    }
}
