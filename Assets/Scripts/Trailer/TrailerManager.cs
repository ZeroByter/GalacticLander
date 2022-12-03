using UnityEngine;
using UnityEngine.SceneManagement;

public class TrailerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject staticCameraTarget;

    private int stage;

    private void Start()
    {
        MainCameraController.ForcePosition();
    }

    private void Update()
    {
        var playerShip = PlayerShipController.Singletron;

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Trailer");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(stage == 0)
            {
                var missilePosition = new Vector3(-3.5f, -0.06f);
                var directionToShip = playerShip.transform.position - missilePosition;

                MissileController newMissile = Instantiate(Resources.Load<GameObject>("Missile/Missile")).GetComponent<MissileController>();
                newMissile.transform.position = missilePosition;
                newMissile.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(directionToShip.x, directionToShip.y) * Mathf.Rad2Deg * -1);
                newMissile.gameObject.SetActive(true);
                //newMissile.missileLauncher = this;
            }
            else if(stage == 1)
            {
                Destroy(staticCameraTarget);
                MainCameraController.AddTarget(PlayerShipController.Singletron);
            }
            else if(stage == 2)
            {
                //spawn crates lol
                Vector2[] spawns = new Vector2[] {
                    new Vector2(9.5f, 7f),
                    new Vector2(9f, 7.25f),
                    new Vector2(10.62f, 6.5f),
                };
                float[] rotations = new float[] { 12f, -45f, 39 };

                for(int i = 0; i < spawns.Length; i++)
                {
                    GameObject newCrate = Instantiate(Resources.Load<GameObject>("Crate/Crate"), spawns[i], Quaternion.Euler(0, 0, rotations[i]));
                    newCrate.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -1f);
                }
            }

            stage++;
        }
    }
}
