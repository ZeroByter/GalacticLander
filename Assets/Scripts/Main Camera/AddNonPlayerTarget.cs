using UnityEngine;

public class AddNonPlayerTarget : MonoBehaviour
{
    private void Start()
    {
        MainCameraController.AddNonPlayerTarget(transform);
    }

    private void OnEnable()
    {
        MainCameraController.AddNonPlayerTarget(transform);
    }

    private void OnDisable()
    {
        MainCameraController.RemoveNonPlayerTarget(transform);
    }

    private void OnDestroy()
    {
        MainCameraController.RemoveNonPlayerTarget(transform);
    }
}
