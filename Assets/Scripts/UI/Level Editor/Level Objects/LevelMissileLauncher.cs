using System;
using UnityEngine;

[Serializable]
public class LevelMissileLauncher : LevelEntity {
    public LevelMissileLauncher(string resourceName) {
        this.resourceName = resourceName;
        this.customProperties = new object[0];
        this.randomResourceNames = new string[0];
    }

    public override void ActivatedEditor(GameObject editorObject) {
        base.ActivatedEditor(editorObject);

        GameObject activationRadius = new GameObject("Launcher activation radius", typeof(SpriteRenderer));
        activationRadius.transform.parent = editorObject.transform;
        activationRadius.transform.localPosition = Vector2.zero;
        activationRadius.transform.localEulerAngles = Vector3.zero;
        activationRadius.transform.localScale = new Vector2(Constants.MissileLauncherTriggerDistance * 2, Constants.MissileLauncherTriggerDistance * 2);
        SpriteRenderer activationRadiusSprite = activationRadius.GetComponent<SpriteRenderer>();
        activationRadiusSprite.sprite = Resources.Load<Sprite>("Missile Detection Range");
        activationRadiusSprite.color = Color.red;

        GameObject killOffRadius = new GameObject("Launcher kill off radius", typeof(SpriteRenderer));
        killOffRadius.transform.parent = editorObject.transform;
        killOffRadius.transform.localPosition = Vector2.zero;
        killOffRadius.transform.localScale = new Vector2(Constants.MissileKillDistance * 2, Constants.MissileKillDistance * 2);
        SpriteRenderer killOffRadiusSprite = killOffRadius.GetComponent<SpriteRenderer>();
        killOffRadiusSprite.sprite = Resources.Load<Sprite>("Missile Range Circle");
        killOffRadiusSprite.color = Color.yellow;
    }
}
