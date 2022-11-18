using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MarchingSquaresManager : MonoBehaviour
{
    private static MarchingSquaresManager Singleton;

    public static void SetData(int width, int height, float[] data)
    {
        if (Singleton == null) return;

        Singleton.width = width;
        Singleton.height = height;
        Singleton.data = data;
    }

    public static void AddValue(int x, int y, float value)
    {
        if (Singleton == null) return;

        var index = x + y * Singleton.width;

        if (index < 0 || index > Singleton.data.Length) return;

        Singleton.data[index] = Mathf.Clamp01(Singleton.data[index] + value);
    }

    //Reverse offset to subtract instead of add
    public static void AddValues(Vector2 center, float size, float offset = 0.6f)
    {
        if (Singleton == null) return;

        var min = new Vector2Int(Mathf.FloorToInt(center.x - size / 2 - Singleton.size * 2), Mathf.FloorToInt(center.y - size / 2 - Singleton.size * 2));
        var max = new Vector2Int(Mathf.CeilToInt(center.x + size / 2 + Singleton.size * 2), Mathf.CeilToInt(center.y + size / 2 + Singleton.size * 2));

        for(int y = min.y; y < max.y; y++)
        {
            for(int x = min.x; x < max.x; x++)
            {
                AddValue(x, y, Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), center) / size * offset));
                AddValue(x + 1, y, Mathf.Clamp01(Vector2.Distance(new Vector2(x + 1, y), center) / size * offset));
                AddValue(x, y + 1, Mathf.Clamp01(Vector2.Distance(new Vector2(x, y + 1), center) / size * offset));
                AddValue(x + 1, y + 1, Mathf.Clamp01(Vector2.Distance(new Vector2(x + 1, y + 1), center) / size * offset));
            }
        }
    }

    public static void GenerateMesh()
    {
        if (Singleton == null) return;

        Singleton._GenerateMesh();
    }

    private MeshFilter meshFilter;

    private Mesh mesh;

    private float isoLevel = 0.5f;
    private float size = 0.1f;

    private int width;
    private int height;

    private float[] data;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;
    }

    private void _GenerateMesh()
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cornerLocations = new Vector3[]
                {
                    new Vector3(x * size, y * size, 0),
                    new Vector3(x * size + size, y * size, 0),
                    new Vector3(x * size, y * size + size, 0),
                    new Vector3(x * size + size, y * size + size, 0)
                };

                var cornerValues = new float[]
                {
                    data[GetCornerIndex(x, y, width)],
                    data[GetCornerIndex(x + 1, y, width)],
                    data[GetCornerIndex(x, y + 1, width)],
                    data[GetCornerIndex(x + 1, y + 1, width)]
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

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    private int GetCornerIndex(int x, int y, int width)
    {
        return x + y * width;
    }

    private int GetSquareIndex(float corner0, float corner1, float corner2, float corner3)
    {
        var index = 0;

        if (corner0 >= isoLevel) index |= 1;
        if (corner1 >= isoLevel) index |= 2;
        if (corner2 >= isoLevel) index |= 4;
        if (corner3 >= isoLevel) index |= 8;

        return index;
    }

    private Vector3 GetPointAlongEdge(Vector3[] p, float[] v, int i1, int i2)
    {
        var p1 = p[i1];
        var p2 = p[i2];
        var v1 = v[i1];
        var v2 = v[i2];

        var mul = (isoLevel - v1) / (v2 - v1);

        return Vector3.Lerp(p1, p2, mul);
    }
}
