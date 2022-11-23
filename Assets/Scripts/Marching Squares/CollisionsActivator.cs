using UnityEngine;

public class CollisionsActivator : MonoBehaviour
{
    private float lastActivatedCollisions;
    private Vector3 lastPos;

    private void FixedUpdate()
    {
        if(Time.time - lastActivatedCollisions > 0.05f)
        {
            var pos = (transform.position + new Vector3(18, 18)) / 36f * 150f;
            var chunkSize = MarchingSquaresManager.ChunkSize;

            MarchingSquaresManager.GenerateCollisions(pos.x, pos.y);

            var movementDirection = (pos - lastPos).normalized * chunkSize;
            
            MarchingSquaresManager.GenerateCollisions(pos.x + movementDirection.x, pos.y + movementDirection.y);

            lastPos = pos;
        }
    }
}
