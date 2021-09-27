using UnityEngine;

public class LevelEditorEntitiesList : MonoBehaviour {
    public GameObject template;

    private void AddListItem(LevelEntity entityData, string spriteName) {
        LevelEditorEntityListItem controller = Instantiate(template, template.transform.parent).GetComponent<LevelEditorEntityListItem>();
        entityData.spriteName = spriteName; //setting the sprite name for level loading purposes
        controller.Setup(entityData, Resources.Load<Sprite>(spriteName));
    }

    private void Awake() {
        template.SetActive(false);

        AddListItem(new LevelEntity("Ship Pads/Ship Sensor Pad") { isLogicEntity = true, isLogicActivator = true }, "Landing Pads/Ship Sensor Pad Sprite");
        AddListItem(new LevelEntity("Door/Door") { lockedToGrid = true, isLogicEntity = true }, "Door/Full Door Sprite");
        AddListItem(new LevelEntity("Crate/Crate"), "Crate/Crate Sprite");
        AddListItem(new LevelEntity("Crate/Crate Sensor") { isLogicEntity = true, isLogicActivator = true }, "Crate/Crate Sensor Sprite");
        AddListItem(new LevelMissileLauncher("Missile/Missile Launcher"), "Missile/Missile Launcher");
        AddListItem(new LevelEntity("Spikes/Spike 0", "Spikes/Spike 1", "Spikes/Spike 2") { lockedToGrid = true, lockedRotation = true }, "Spikes/Spike Editor Sprite");
    }
}
