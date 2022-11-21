using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MarchingSquaresManager : MonoBehaviour
{
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
    }

    public static void SetData(float[] data)
    {
        if (Singleton == null) return;

        Singleton.data = data;
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
    }

    public static void AddValue(int x, int y, float value)
    {
        if (Singleton == null) return;

        if (x < 3 || x > 150 - 3) return;
        if (y < 3 || y > 150 - 3) return;

        var index = x + y * DataWidth;

        if (index < 0 || index > Singleton.data.Length) return;

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

    public static void GenerateCollisions()
    {
        if (Singleton == null) return;

        foreach(Transform child in Singleton.transform)
        {
            Destroy(child.gameObject);
        }

        var collisionsObject = new GameObject();

        foreach(var linkedEdges in Singleton.linkedEdgesList)
        {
            var collider = collisionsObject.AddComponent<EdgeCollider2D>();
            collider.points = linkedEdges.ToArray();
        }

        collisionsObject.transform.parent = Singleton.transform;
        collisionsObject.transform.localPosition = Vector3.zero;
        collisionsObject.transform.localScale = Vector3.one;
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

    public static void GenerateMesh(bool markEdges = false)
    {
        if (Singleton == null) return;

        Singleton._GenerateMesh(markEdges);
    }

    private MeshFilter meshFilter;

    private Mesh mesh;

    private float[] data;

    private List<LinkedList<Vector2>> linkedEdgesList = new List<LinkedList<Vector2>>();

    private void Awake()
    {
        Singleton = this;

        meshFilter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        meshFilter.sharedMesh = mesh;
    }

    private void AddEdgeToEdges(bool markEdges, params Vector2[] edges)
    {
        if (!markEdges) return;

        var foundLinked = false;

        foreach (var linkedEdges in linkedEdgesList)
        {
            for (int i = 0; i < edges.Length; i++)
            {
                Vector2 edge = edges[i];

                var foundEdge = linkedEdges.Find(edge);

                if (foundEdge != null)
                {
                    if(i == 0)
                    {
                        linkedEdges.AddAfter(foundEdge, edges[1]);
                    }
                    else
                    {
                        linkedEdges.AddAfter(foundEdge, edges[0]);
                    }

                    foundLinked = true;
                }
            }
        }

        if (!foundLinked)
        {
            var newLinkedList = new LinkedList<Vector2>();

            foreach (var edge in edges)
            {
                newLinkedList.AddLast(edge);
            }

            linkedEdgesList.Add(newLinkedList);
        }
    }

    private void _GenerateMesh(bool markEdges)
    {
        mesh.Clear();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (int y = 0; y < DataHeight; y++)
        {
            for (int x = 0; x < DataWidth; x++)
            {
                var cornerLocations = new Vector3Int[]
                {
                    new Vector3Int(x, y, 0),
                    new Vector3Int(x + 1, y, 0),
                    new Vector3Int(x, y + 1, 0),
                    new Vector3Int(x + 1, y + 1, 0)
                };

                var cornerValues = new float[]
                {
                    data[GetCornerIndex(x, y)],
                    data[GetCornerIndex(x + 1, y)],
                    data[GetCornerIndex(x, y + 1)],
                    data[GetCornerIndex(x + 1, y + 1)]
                };

                var cubeIndex = GetSquareIndex(cornerValues[0], cornerValues[1], cornerValues[2], cornerValues[3]);

                var verticesCount = vertices.Count;

                #region Big ugly if-statements for generating squares
                if (cubeIndex == 15)
                {
                    vertices.Add(cornerLocations[0]);
                    vertices.Add(cornerLocations[1]);
                    vertices.Add(cornerLocations[2]);
                    vertices.Add(cornerLocations[3]);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 2);

                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 2);
                    triangles.Add(verticesCount + 1);
                }
                else if (cubeIndex == 1)
                {
                    var c1 = cornerLocations[0];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 2);
                }
                else if (cubeIndex == 2)
                {
                    var c1 = cornerLocations[1];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 2);
                    triangles.Add(verticesCount + 1);
                }
                else if (cubeIndex == 3)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[1];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);
                    vertices.Add(c2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 1);

                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 2);
                    triangles.Add(verticesCount + 1);
                }
                else if (cubeIndex == 4)
                {
                    var c1 = cornerLocations[2];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 2);
                }
                else if (cubeIndex == 5)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[2];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);
                    vertices.Add(c2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 3);

                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 2);
                }
                else if (cubeIndex == 6)
                {
                    var c1 = cornerLocations[1];
                    var c2 = cornerLocations[2];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);
                    var p3 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p4 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    AddEdgeToEdges(markEdges, p1, p2);
                    AddEdgeToEdges(markEdges, p3, p4);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    vertices.Add(c2);
                    vertices.Add(p3);
                    vertices.Add(p4);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 2);
                    triangles.Add(verticesCount + 1);

                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 4);
                    triangles.Add(verticesCount + 5);
                }
                else if (cubeIndex == 7)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[1];
                    var c3 = cornerLocations[2];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(c2);
                    vertices.Add(c3);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 4);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 4);
                    triangles.Add(verticesCount + 3);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 2);
                }
                else if (cubeIndex == 8)
                {
                    var c1 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 2);
                    triangles.Add(verticesCount + 1);
                }
                else if (cubeIndex == 9)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p3 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);
                    var p4 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    AddEdgeToEdges(markEdges, p1, p2);
                    AddEdgeToEdges(markEdges, p3, p4);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    vertices.Add(c2);
                    vertices.Add(p3);
                    vertices.Add(p4);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 2);

                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 5);
                    triangles.Add(verticesCount + 4);
                }
                else if (cubeIndex == 10)
                {
                    var c1 = cornerLocations[1];
                    var c2 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);
                    vertices.Add(c2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 1);

                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 2);
                    triangles.Add(verticesCount + 1);
                }
                else if (cubeIndex == 11)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[1];
                    var c3 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(c2);
                    vertices.Add(c3);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 3);

                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 2);
                    triangles.Add(verticesCount + 4);

                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 4);
                    triangles.Add(verticesCount + 3);
                }
                else if (cubeIndex == 12)
                {
                    var c1 = cornerLocations[2];
                    var c2 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c1);
                    vertices.Add(p1);
                    vertices.Add(p2);
                    vertices.Add(c2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 3);

                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 2);
                }
                else if (cubeIndex == 13)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[2];
                    var c3 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c2);
                    vertices.Add(c1);
                    vertices.Add(c3);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 1);
                    triangles.Add(verticesCount + 3);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 4);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 4);
                    triangles.Add(verticesCount + 2);
                }
                else if (cubeIndex == 14)
                {
                    var c1 = cornerLocations[1];
                    var c2 = cornerLocations[2];
                    var c3 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);

                    AddEdgeToEdges(markEdges, p1, p2);

                    vertices.Add(c3);
                    vertices.Add(c1);
                    vertices.Add(c2);
                    vertices.Add(p1);
                    vertices.Add(p2);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 3);
                    triangles.Add(verticesCount + 1);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 4);
                    triangles.Add(verticesCount + 3);

                    triangles.Add(verticesCount);
                    triangles.Add(verticesCount + 2);
                    triangles.Add(verticesCount + 4);
                }
                #endregion
            }
        }

        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    private int GetCornerIndex(int x, int y)
    {
        return x + y * DataWidth;
    }

    private int GetSquareIndex(float corner0, float corner1, float corner2, float corner3)
    {
        var index = 0;

        if (corner0 >= IsoLevel) index |= 1;
        if (corner1 >= IsoLevel) index |= 2;
        if (corner2 >= IsoLevel) index |= 4;
        if (corner3 >= IsoLevel) index |= 8;

        return index;
    }

    private Vector3 GetPointAlongEdge(Vector3Int[] p, float[] v, int i1, int i2)
    {
        var p1 = p[i1];
        var p2 = p[i2];
        var v1 = v[i1];
        var v2 = v[i2];

        var mul = (IsoLevel - v1) / (v2 - v1);

        return Vector3.Lerp(p1, p2, mul);
    }
}
