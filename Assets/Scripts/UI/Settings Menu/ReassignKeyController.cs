using UnityEngine;
using TMPro;
using static KeysManager;

public class ReassignKeyController : MonoBehaviour
{
    [Header("UI")]
    public GameObject raycasterBlockerCanvas;
    [Header("The key")]
    public Player player;
    public Key key;

    private TMP_Text triggerButtonLabel;

    private bool listeningForInput = false;

    private KeyCode GetKeyCode()
    {
        return GetKey(player, key);
    }

    private void Awake()
    {
        triggerButtonLabel = GetComponentInChildren<TMP_Text>();

        triggerButtonLabel.text = GetKeyCode().ToString();
    }

    public void ListenForInput()
    {
        if (GetKeyCode() != KeyCode.None) //if key is set to something
        {
            SetKey(player, key, KeyCode.None); //set key to nothing
            triggerButtonLabel.text = GetKeyCode().ToString(); //set text to say 'None'
            return;
        }

        listeningForInput = true;

        triggerButtonLabel.text = "<Listening For Key Or Axis>";
        raycasterBlockerCanvas.SetActive(true);
    }

    private void Update()
    {
        if (listeningForInput)
        {
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(vKey))
                {
                    listeningForInput = false; //no longer listening to keys
                    SetKey(player, key, vKey); //set key
                    triggerButtonLabel.text = GetKeyCode().ToString(); //set text to show the key has been set
                    raycasterBlockerCanvas.SetActive(false); //disable raycasterBlocker
                }
            }
        }
    }
}
