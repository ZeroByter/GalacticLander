using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarchingSquaresManager : MonoBehaviour
{
    public static int ChunkSize = 25;

    public static int DataWidth = 150;
    public static int DataHeight = 150;
    public static float IsoLevel = 0.5f;

    private static MarchingSquaresManager Singleton;

    public static void CreateBlank()
    {
        if (Singleton == null) return;

        Singleton.data = new float[DataWidth * DataHeight + DataWidth + 1];
        for (int i = 0; i < Singleton.data.Length; i++)
        {
            Singleton.data[i] = 1f;
        }

        var holeWidth = 40;
        var holeHeight = 15;
        var center = new Vector2(DataWidth / 2 + 9, DataHeight / 2 + holeHeight / 2);

        var min = new Vector2Int(Mathf.FloorToInt(center.x - holeWidth / 2), Mathf.FloorToInt(center.y - holeHeight / 2));
        var max = new Vector2Int(Mathf.CeilToInt(center.x + holeWidth / 2), Mathf.CeilToInt(center.y + holeHeight / 2));

        for (int y = min.y; y < max.y; y++)
        {
            for (int x = min.x; x < max.x; x++)
            {
                AddValue(x, y, Mathf.Lerp(-0.5f, -1f, Mathf.InverseLerp(min.y, max.y, y)));
            }
        }

        MarkAllChunksDirty();
    }

    public static void SetData(float[] data)
    {
        if (Singleton == null) return;

        Singleton.data = data;

        MarkAllChunksDirty();
    }

    private static Vector2 ConvertPositionFromOldToNew(Vector2 position)
    {
        return new Vector2(position.x / 36f * 150f, position.y / 36f * 150f);
    }

    public static void SetDataFromOldLevel(LevelData oldLevelData)
    {
        var bounds = oldLevelData.GetBounds();

        Singleton.data = new float[DataWidth * DataHeight + DataWidth + 1];
        for (int i = 0; i < Singleton.data.Length; i++)
        {
            Singleton.data[i] = 1f;
        }

        for (int y = Mathf.FloorToInt(bounds.min.y); y < Mathf.FloorToInt(bounds.max.y); y++)
        {
            for (int x = Mathf.FloorToInt(bounds.min.x); x < Mathf.FloorToInt(bounds.max.x); x++)
            {
                if (oldLevelData.IsPointInLevel(new Vector2(x, y)))
                {
                    var newPosition = ConvertPositionFromOldToNew(new Vector2(x, y));

                    AddValuesSquare(new Vector2(DataWidth / 2 + Mathf.RoundToInt(newPosition.x), DataHeight / 2 + Mathf.RoundToInt(newPosition.y)), 6, 6, -1f);
                }
            }
        }

        MarkAllChunksDirty();
    }

    //x and y are of the real data point, not chunk coordinates
    public static void MarkChunkDirty(float x, float y)
    {
        if (Singleton == null) return;

        var chunkX = Mathf.FloorToInt(x / ChunkSize);
        var chunkY = Mathf.FloorToInt(y / ChunkSize);

        var numberOfChunks = DataWidth / ChunkSize;

        Singleton.chunks[chunkX + chunkY * numberOfChunks].MarkDirty();
    }

    public static void MarkAllChunksDirty()
    {
        if (Singleton == null) return;

        foreach(var chunk in Singleton.chunks)
        {
            chunk.MarkDirty();
        }
    }

    public static void AddValue(int x, int y, float value)
    {
        if (Singleton == null) return;

        if (x < 3 || x > 150 - 3) return;
        if (y < 3 || y > 150 - 3) return;

        var index = x + y * DataWidth;

        if (index < 0 || index > Singleton.data.Length) return;

        MarkChunkDirty(x, y);

        Singleton.data[index] = Mathf.Clamp01(Singleton.data[index] + value);
    }

    //Reverse offset to subtract instead of add
    public static void AddValues(Vector2 center, float size, float offset)
    {
        if (Singleton == null) return;

        var min = new Vector2Int(Mathf.FloorToInt(center.x - size / 2), Mathf.FloorToInt(center.y - size / 2));
        var max = new Vector2Int(Mathf.CeilToInt(center.x + size / 2), Mathf.CeilToInt(center.y + size / 2));

        for (int y = min.y; y < max.y; y++)
        {
            for (int x = min.x; x < max.x; x++)
            {
                AddValue(x, y, (1 - Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), center) / size * 2)) * offset);
                AddValue(x + 1, y, (1 - Mathf.Clamp01(Vector2.Distance(new Vector2(x + 1, y), center) / size * 2)) * offset);
                AddValue(x, y + 1, (1 - Mathf.Clamp01(Vector2.Distance(new Vector2(x, y + 1), center) / size * 2)) * offset);
                AddValue(x + 1, y + 1, (1 - Mathf.Clamp01(Vector2.Distance(new Vector2(x + 1, y + 1), center) / size * 2)) * offset);
            }
        }
    }

    public static void SetColor(Color color)
    {
        if (Singleton == null) return;

        Singleton.GetComponent<MeshRenderer>().sharedMaterial.color = color;
    }

    private static void AddValuesSquare(Vector2 center, int width, int height, float value)
    {
        if (Singleton == null) return;

        var min = new Vector2Int(Mathf.FloorToInt(center.x - width / 2), Mathf.FloorToInt(center.y - height / 2));
        var max = new Vector2Int(Mathf.CeilToInt(center.x + width / 2), Mathf.CeilToInt(center.y + height / 2));

        for (int y = min.y; y < max.y; y++)
        {
            for (int x = min.x; x < max.x; x++)
            {
                AddValue(x, y, value);
            }
        }
    }

    public static float[] GetValues()
    {
        if (Singleton == null) return new float[0];

        return Singleton.data;
    }

    public static void GenerateMeshAndCollisions()
    {
        if (Singleton == null) return;

        foreach (var chunk in Singleton.chunks)
        {
            chunk.GenerateMeshAndCollisions(true, false);
        }
    }

    public static void GenerateCollisions(float x, float y)
    {
        if (Singleton == null) return;

        var chunkX = Mathf.FloorToInt(x / ChunkSize);
        var chunkY = Mathf.FloorToInt(y / ChunkSize);

        var numberOfChunks = DataWidth / ChunkSize;
        var index = chunkX + chunkY * numberOfChunks;

        if (index < 0 || index >= numberOfChunks * numberOfChunks) return;

        Singleton.chunks[index].GenerateCollisions();
    }

    private float[] data;

    private MarchingSquaresChunkController[] chunks;

    [SerializeField]
    private Material meshMaterial;

    private void Awake()
    {
        Singleton = this;

        var numberOfChunks = DataWidth / ChunkSize;

        chunks = new MarchingSquaresChunkController[numberOfChunks * numberOfChunks];
        for (int y = 0; y < numberOfChunks; y++)
        {
            for(int x = 0; x < numberOfChunks; x++)
            {
                var chunk = new GameObject($"Chunk-{x},{y}");

                var meshRenderer = chunk.AddComponent<MeshRenderer>();
                meshRenderer.material = meshMaterial;

                var chunkController = chunk.AddComponent<MarchingSquaresChunkController>();
                chunkController.Initialize(x, y);

                chunk.transform.parent = transform;
                chunk.transform.localPosition = new Vector2(x, y) * ChunkSize;
                chunk.transform.localScale = Vector3.one;

                chunk.layer = gameObject.layer;
                chunk.tag = gameObject.tag;

                chunks[x + y * numberOfChunks] = chunkController;
            }
        }
    }
}
