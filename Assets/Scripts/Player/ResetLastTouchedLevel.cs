using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ComponentHealth = ShipComponentController.ComponentHealth;

public class ResetLastTouchedLevel : MonoBehaviour {
    public NetworkObject parentNObject;
    public ShipComponentController shipComponent;

    private List<string> touchingTags = new List<string>();
    private Vector2 startPosition;

    private bool IsDestroyed() {
        if (shipComponent == null) return false;

        return shipComponent.GetHealth() == ComponentHealth.Destroyed;
    }

    private void Start() {
        if (NetworkingManager.CurrentLobbyValid) { //if we are in a lobby
            if (parentNObject != null && !parentNObject.IsMine()) Destroy(this); //if the network object is not null and it's not ours, we destroy the compoennt
        }

        startPosition = transform.position;
        touchingTags.Add("startTouchingTag");
    }

    private void FixedUpdate() {
        if(Vector2.Distance(startPosition, transform.position) > 1.5f) {
            touchingTags.Remove("startTouchingTag");
        }

        //print(string.Format("{0} - {1} && {2} && {3}", gameObject.name, !IsDestroyed(), touchingTags.Count > 0, PlayerShipController.Singletron != null));

        if (!IsDestroyed() && touchingTags.Count > 0 && PlayerShipController.Singletron != null) {
            PlayerShipController.Singletron.lastTouchedLevel = Time.time;

            if (PlayerShipController.Singletron.positionsRecorded == null) PlayerShipController.Singletron.positionsRecorded = new List<PositionRecord>();
            PlayerShipController.Singletron.positionsRecorded.Clear();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        touchingTags.Remove("startTouchingTag");

        if (!touchingTags.Contains(collision.gameObject.tag)) touchingTags.Add(collision.gameObject.tag);
    }

    private void OnCollisionExit2D(Collision2D collision) {
        touchingTags.Remove("startTouchingTag");

        touchingTags.Remove(collision.gameObject.tag);
    }
}
