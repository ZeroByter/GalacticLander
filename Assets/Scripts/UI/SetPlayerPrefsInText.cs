using TMPro;
using UnityEngine;

public class SetPlayerPrefsInText : MonoBehaviour {
    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();

        text.text = text.text.Replace("{Normal_LeftEngine}", KeysManager.GetKey(KeysManager.Player.Normal, KeysManager.Key.LeftEngine).ToString());
        text.text = text.text.Replace("{Normal_RightEngine}", KeysManager.GetKey(KeysManager.Player.Normal, KeysManager.Key.RightEngine).ToString());
        text.text = text.text.Replace("{Normal_Crate}", KeysManager.GetKey(KeysManager.Player.Normal, KeysManager.Key.Crate).ToString());
        text.text = text.text.Replace("{Splitscreen_LeftEngine}", KeysManager.GetKey(KeysManager.Player.Splitscreen, KeysManager.Key.LeftEngine).ToString());
        text.text = text.text.Replace("{Splitscreen_RightEngine}", KeysManager.GetKey(KeysManager.Player.Splitscreen, KeysManager.Key.RightEngine).ToString());
        text.text = text.text.Replace("{Splitscreen_Crate}", KeysManager.GetKey(KeysManager.Player.Splitscreen, KeysManager.Key.Crate).ToString());
    }
}
