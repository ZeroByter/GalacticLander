using UnityEngine;

public class CollisionsActivator : MonoBehaviour
{
    [SerializeField]
    private Vector3 positionOffset = new Vector3(18,18);
    [SerializeField]
    private float levelSceneSize = 36f;

    private float lastActivatedCollisions;
    
    private void FixedUpdate()
    {
        if(Time.time - lastActivatedCollisions > 0.05f)
        {
            var pos = (transform.position + positionOffset) / levelSceneSize * 150f;
            var chunkSize = MarchingSquaresManager.ChunkSize / 2;

            MarchingSquaresManager.GenerateCollisions(pos.x, pos.y);
            MarchingSquaresManager.GenerateCollisions(pos.x + chunkSize, pos.y);
            MarchingSquaresManager.GenerateCollisions(pos.x - chunkSize, pos.y);
            MarchingSquaresManager.GenerateCollisions(pos.x, pos.y + chunkSize);
            MarchingSquaresManager.GenerateCollisions(pos.x, pos.y - chunkSize);

            MarchingSquaresManager.GenerateCollisions(pos.x + chunkSize, pos.y + chunkSize);
            MarchingSquaresManager.GenerateCollisions(pos.x - chunkSize, pos.y + chunkSize);
            MarchingSquaresManager.GenerateCollisions(pos.x + chunkSize, pos.y - chunkSize);
            MarchingSquaresManager.GenerateCollisions(pos.x - chunkSize, pos.y - chunkSize);
        }
    }
}
