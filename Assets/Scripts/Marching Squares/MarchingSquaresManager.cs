using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MarchingSquaresManager : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private float isoLevel = 0.5f;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        var width = 200;
        var height = 100;
        var size = 0.1f;
        var cornerData = new float[width * height + width + 1];

        cornerData[GetCornerIndex(50, 50, width)] = 1f;
        cornerData[GetCornerIndex(50, 51, width)] = 0.5f;
        cornerData[GetCornerIndex(51, 50, width)] = 0.5f;
        cornerData[GetCornerIndex(51, 51, width)] = 0.5f;

        var mesh = new Mesh();

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
                    cornerData[GetCornerIndex(x, y, width)],
                    cornerData[GetCornerIndex(x + 1, y, width)],
                    cornerData[GetCornerIndex(x, y + 1, width)],
                    cornerData[GetCornerIndex(x + 1, y + 1, width)]
                };

                var cubeIndex = GetSquareIndex(cornerValues[0], cornerValues[1], cornerValues[2], cornerValues[3]);

                var verticesCount = vertices.Count;

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
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        meshFilter.sharedMesh = mesh;
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
