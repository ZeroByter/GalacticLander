using UnityEngine;

public class LevelEditorEntitiesList : MonoBehaviour {
    public GameObject template;

    private void AddListItem(LevelEntity entityData, string spriteName, string displayName) {
        LevelEditorEntityListItem controller = Instantiate(template, template.transform.parent).GetComponent<LevelEditorEntityListItem>();
        entityData.spriteName = spriteName; //setting the sprite name for level loading purposes
        controller.Setup(entityData, Resources.Load<Sprite>(spriteName), displayName);
    }

    private void Awake() {
        template.SetActive(false);

        AddListItem(new LevelEntity("Ship Pads/Ship Sensor Pad") { isLogicEntity = true, isLogicActivator = true }, "Landing Pads/Ship Sensor Pad Sprite", "Ship Sensor");
        AddListItem(new LevelEntity("Door/Door") { isLogicEntity = true }, "Door/Full Door Sprite", "Door");
        AddListItem(new LevelEntity("Crate/Crate"), "Crate/Crate Sprite", "Crate");
        AddListItem(new LevelEntity("Crate/Crate Sensor") { isLogicEntity = true, isLogicActivator = true }, "Crate/Crate Sensor Sprite", "Crate Sensor");
        AddListItem(new LevelMissileLauncher("Missile/Missile Launcher"), "Missile/Missile Launcher", "Missile Launcher");
        AddListItem(new LevelEntity("Spikes/Spike 0", "Spikes/Spike 1", "Spikes/Spike 2"), "Spikes/Spike Editor Sprite", "Spike");
    }
}
