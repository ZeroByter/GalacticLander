using UnityEngine;

public class DoScreenShake : MonoBehaviour
{
    public float magnitude;
    public float duration;

    private void Awake()
    {
        MainCameraController.StartShake(magnitude, duration);
    }
}
