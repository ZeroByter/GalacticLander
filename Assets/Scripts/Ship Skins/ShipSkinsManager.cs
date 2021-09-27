using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSkin {
    public string name;
    public string resourceSuffix;
    public string requiredGameAchievement;
    public bool developerOnly;

    public ShipSkin(string name, string resourceSuffix) {
        this.name = name;
        this.resourceSuffix = resourceSuffix;
        requiredGameAchievement = "";
    }

    public ShipSkin(string name, string resourceSuffix, string requiredGameAchievement) {
        this.name = name;
        this.resourceSuffix = resourceSuffix;
        this.requiredGameAchievement = requiredGameAchievement;
    }

    public Sprite GetSprite() {
        return Resources.Load<Sprite>("Ship Skins/mainShip" + resourceSuffix);
    }
}

public class ShipSkinsManager : MonoBehaviour {
    public static int SelectedSkin {
        get {
            return PlayerPrefs.GetInt("selectedSkin", 0);
        }
        set {
            PlayerPrefs.SetInt("selectedSkin", value);
        }
    }

    public static List<ShipSkin> Skins = new List<ShipSkin>();

    public static ShipSkinsManager Singletron;

    private void Awake() {
        if(Singletron != null) {
            Destroy(gameObject);
            return;
        }

        Singletron = this;
        DontDestroyOnLoad(gameObject);

        Skins.Add(new ShipSkin("normal", ""));
        Skins.Add(new ShipSkin("halloweenEvent", "Halloween", "halloween"));
        Skins.Add(new ShipSkin("developerOnly", "Developer") { developerOnly = true });
        Skins.Add(new ShipSkin("landerRemade", "LanderRemade", "landerRemade"));
    }
}
