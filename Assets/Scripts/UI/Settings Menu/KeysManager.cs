using UnityEngine;

public class KeysManager : MonoBehaviour {
    public enum Player
    {
        Normal, Splitscreen
    }
    public enum Key
    {
        LeftEngine, RightEngine, Crate
    }

    private static string GetKeyFormat(Player player, Key key)
    {
        return string.Format("{0}_{1}", player.ToString(), key.ToString());
    }

    public static void SetKey(Player player, Key key, KeyCode keyCode)
    {
        PlayerPrefs.SetInt(GetKeyFormat(player, key), (int)keyCode);
    }

    private static KeyCode GetDefaultKey(Player player, Key key)
    {
        if(player == Player.Normal)
        {
            if(key == Key.LeftEngine)
            {
                return KeyCode.A;
            }
            else if(key == Key.RightEngine)
            {
                return KeyCode.D;
            }
            else
            {
                return KeyCode.X;
            }
        }
        else
        {
            if (key == Key.LeftEngine)
            {
                return KeyCode.LeftArrow;
            }
            else if (key == Key.RightEngine)
            {
                return KeyCode.RightArrow;
            }
            else
            {
                return KeyCode.M;
            }
        }
    }

    public static KeyCode GetKey(Player player, Key key)
    {
        return (KeyCode)PlayerPrefs.GetInt(GetKeyFormat(player, key), (int)GetDefaultKey(player, key));
    }
}
