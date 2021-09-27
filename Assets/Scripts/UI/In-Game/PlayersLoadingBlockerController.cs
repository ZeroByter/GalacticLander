using UnityEngine;

public class PlayersLoadingBlockerController : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private bool allPlayersLoaded;

    private void Awake()
    {
        if (!NetworkingManager.CurrentLobbyValid)
        {
            Destroy(gameObject);
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();

        CursorController.AddUser("waitingForPlayersBlocker");
    }

    private void Update()
    {
        if(!allPlayersLoaded && PlayerShipController.Ships.Count == 2)
        {
            allPlayersLoaded = true;
        }

        if (allPlayersLoaded && Time.timeSinceLevelLoad > 0.75f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, 0.1f);
            if(canvasGroup.alpha < 0.025f)
            {
                Destroy(gameObject);
            }
        }
    }
}
