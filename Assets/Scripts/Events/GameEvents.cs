using SourceConsole;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameEvent {
    //names
    public string name;
    public string displayName;

    //dates
    public DateTime startDate;
    public DateTime endDate;

    //descriptions
    public string description;

    //number of levels
    public int numberOfLevels;

    //theme color
    public Color themeColor;
    
    //get methods
    public Sprite GetBannerResource() {
        return Resources.Load<Sprite>(name + "/banner");
    }

    public virtual Color GetTileColor(float x, float y)
    {
        return themeColor;
    }
}

public class LanderRemadeGameEvent : GameEvent
{
    public override Color GetTileColor(float x, float y)
    {
        //TODO: random noise color based on x and y (so that all the tiles will have a rainbow-like color effect across the level (use perlin noise obviously)

        float scale = 0.05f;
        x *= scale;
        y *= scale;

        float max = 0.7f;
        float min = 0.125f;

        float r = Mathf.Lerp(min, max, Mathf.PerlinNoise(x + 200 + LevelLoader.CurrentLevelSeed, y + 200 + LevelLoader.CurrentLevelSeed));
        float g = Mathf.Lerp(min, max, Mathf.PerlinNoise(x + 201 + LevelLoader.CurrentLevelSeed, y + 201 + LevelLoader.CurrentLevelSeed));
        float b = Mathf.Lerp(min, max, Mathf.PerlinNoise(x + 202 + LevelLoader.CurrentLevelSeed, y + 202 + LevelLoader.CurrentLevelSeed));
        
        return new Color(r, g, b);
        //return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        //return base.GetTileColor(x, y);
    }
}

public class GameEvents : MonoBehaviour {
    [ConVar]
    public static int CurrentEventProgression {
        get {
            return PlayerPrefs.GetInt($"currentEventProgression_{GetCurrentEvent().name}", 0);
        }
        set {
            PlayerPrefs.SetInt($"currentEventProgression_{GetCurrentEvent().name}", value);
        }
    }
    [ConVar]
    public static int CurrentEventCoopProgression {
        get {
            return PlayerPrefs.GetInt($"currentEventCoopProgression_{GetCurrentEvent().name}", 0);
        }
        set {
            PlayerPrefs.SetInt($"currentEventCoopProgression_{GetCurrentEvent().name}", value);
        }
    }

    private static GameEvents Singletron;
    [ConVar]
    public static string ForceDebugEvent {
        get
        {
            if (IsUserDebugger.GetIsUserDebugger())
            {
                return Singletron._forceDebugEvent;
            }
            else
            {
                return "";
            }
        }
        set
        {
            Singletron._forceDebugEvent = value;
        }
    }

    public string _forceDebugEvent;

    private List<GameEvent> events = new List<GameEvent>();

    public static Color GetColorFromHex(string hex) {
        Color returnColor;
        if(ColorUtility.TryParseHtmlString(hex, out returnColor)) {
            return returnColor;
        } else {
            return Color.white;
        }
    }

    private void Awake() {
        if(Singletron != null) {
            Destroy(gameObject);
            return;
        }

        Singletron = this;
        DontDestroyOnLoad(gameObject);

        //adding all hard-coded events...
        GameEvent halloweenEvent = new GameEvent();
        halloweenEvent.name = "halloween";
        halloweenEvent.displayName = "Halloween";
        halloweenEvent.startDate = new DateTime(DateTime.Now.Year, 10, 24);
        halloweenEvent.endDate = new DateTime(DateTime.Now.Year, 11, 1);
        halloweenEvent.numberOfLevels = 10;
        halloweenEvent.themeColor = GetColorFromHex("#772D00");
        halloweenEvent.description = "Welcome to the Halloween Special Galactic Mission!\n\n" +
            "In celebration of Halloween, we have launched this galactic mission to explore the scary and spooky mysteries of the galaxy!\n\n" +
            "Complete the galactic mission, and get a sweet new ship skin that you can mess around with in singleplayer, or show off to a friend in co-op mode!";
        events.Add(halloweenEvent);

        LanderRemadeGameEvent landerRemadeEvent = new LanderRemadeGameEvent();
        landerRemadeEvent.name = "landerRemade";
        landerRemadeEvent.displayName = "Lander Remade";
        landerRemadeEvent.startDate = new DateTime(DateTime.Now.Year, 5, 5);
        landerRemadeEvent.endDate = new DateTime(DateTime.Now.Year, 5, 15);
        landerRemadeEvent.numberOfLevels = 10;
        landerRemadeEvent.themeColor = GetColorFromHex("#088c8e");
        landerRemadeEvent.description = "Welcome to the Lander Remade Galactic Mission!\n\n" +
            "To commorate the 'remaking' and addition of new features for Galactic Lander, we have launched this galactic mission to give you a new, exciting playing expierence!\n\n" +
            "Complete the galactic mission, and get a sweet new ship skin that you can mess around with in singleplayer, or show off to a friend in co-op mode! It's very shiny.";
        events.Add(landerRemadeEvent);
    }

    //static functions
    public static GameEvent GetCurrentEvent() {
        if (Singletron == null || Singletron.events == null) return null;

        DateTime nowDate = DateTime.Now;

        foreach(GameEvent gameEvent in Singletron.events) {
            if(gameEvent.name == ForceDebugEvent || nowDate >= gameEvent.startDate && nowDate <= gameEvent.endDate) {
                return gameEvent;
            }
        }

        return null;
    }

    public static string NormalLevelNameToEventName(string levelName)
    {
        GameEvent currentEvent = GetCurrentEvent();

        if (currentEvent != null)
        {
            return string.Format("{0}/{0}_", currentEvent.name) + levelName;
        }
        else
        {
            return levelName;
        }
    }
}
