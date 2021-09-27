using UnityEngine;

public class LevelEditorLineController : MonoBehaviour {
    public Transform origin;
    public Transform target;

    private LineRenderer line;

    private void Awake() {
        line = GetComponent<LineRenderer>();
    }

    public void Setup(Transform origin, Transform target) {
        this.origin = origin;
        this.target = target;

        gameObject.SetActive(true);
    }

    private void Update() {
        if(origin == null) {
            Destroy(gameObject);
            return;
        }

        Vector3[] positions = new Vector3[target == null ? 1 : 2];
        if (origin == null || target == null) return;
        positions[0] = origin.position - new Vector3(0,0,1);
        positions[1] = target.position - new Vector3(0, 0, 1);

        line.SetPositions(positions);
    }
}
