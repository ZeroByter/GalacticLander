using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MarchingSquaresChunkController : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh mesh;

    private int x;
    private int y;

    private bool meshDirty = false;
    [SerializeField]
    private bool collisionsDirty = false;

    private HashSet<LinkedList<Vector2>> edgePairsList = new HashSet<LinkedList<Vector2>>();
    private HashSet<LinkedList<Vector2>> linkedEdgesList = new HashSet<LinkedList<Vector2>>();

    public void Initialize(int x, int y)
    {
        this.x = x;
        this.y = y;

        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;
    }

    public void MarkDirty()
    {
        this.meshDirty = true;
        this.collisionsDirty = true;
    }

    private void AddEdgesPairToList(params Vector2[] edges)
    {
        edgePairsList.Add(new LinkedList<Vector2>(edges));
    }

    public void GenerateCollisions()
    {
        if (!collisionsDirty) return;

        GenerateMeshAndCollisions(false, true);

        var collisionsObject = new GameObject();
        foreach (var linkedEdges in linkedEdgesList)
        {
            if (linkedEdges.Count == 0) continue;

            var collider = collisionsObject.AddComponent<EdgeCollider2D>();
            collider.points = linkedEdges.ToArray();
        }
        collisionsObject.transform.parent = transform;
        collisionsObject.transform.localPosition = Vector3.zero;
        collisionsObject.transform.localScale = Vector3.one;
    }

    public void GenerateMeshAndCollisions(bool generateMesh, bool generateCollisions)
    {
        if (generateMesh)
        {
            if (!meshDirty) return;
            mesh.Clear();
        }
        if (generateCollisions && !collisionsDirty) return;

        var data = MarchingSquaresManager.GetValues();
        var chunkSize = MarchingSquaresManager.ChunkSize;

        var minX = x * chunkSize;
        var minY = y * chunkSize;

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (int y = minY; y < minY + chunkSize; y++)
        {
            for (int x = minX; x < minX + chunkSize; x++)
            {
                var cornerLocations = new Vector3Int[]
                {
                    new Vector3Int(x - minX, y - minY, 0),
                    new Vector3Int(x - minX + 1, y - minY, 0),
                    new Vector3Int(x - minX, y - minY + 1, 0),
                    new Vector3Int(x - minX + 1, y - minY + 1, 0)
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
                    if (generateMesh)
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
                }
                else if (cubeIndex == 1)
                {
                    var c1 = cornerLocations[0];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
                        vertices.Add(c1);
                        vertices.Add(p1);
                        vertices.Add(p2);

                        triangles.Add(verticesCount);
                        triangles.Add(verticesCount + 1);
                        triangles.Add(verticesCount + 2);
                    }
                }
                else if (cubeIndex == 2)
                {
                    var c1 = cornerLocations[1];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
                        vertices.Add(c1);
                        vertices.Add(p1);
                        vertices.Add(p2);

                        triangles.Add(verticesCount);
                        triangles.Add(verticesCount + 2);
                        triangles.Add(verticesCount + 1);
                    }
                }
                else if (cubeIndex == 3)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[1];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
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
                }
                else if (cubeIndex == 4)
                {
                    var c1 = cornerLocations[2];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
                        vertices.Add(c1);
                        vertices.Add(p1);
                        vertices.Add(p2);

                        triangles.Add(verticesCount);
                        triangles.Add(verticesCount + 1);
                        triangles.Add(verticesCount + 2);
                    }
                }
                else if (cubeIndex == 5)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[2];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
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
                }
                else if (cubeIndex == 6)
                {
                    var c1 = cornerLocations[1];
                    var c2 = cornerLocations[2];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);
                    var p3 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p4 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                        AddEdgesPairToList(p3, p4);
                    }
                    if (generateMesh)
                    {
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
                }
                else if (cubeIndex == 7)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[1];
                    var c3 = cornerLocations[2];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
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
                }
                else if (cubeIndex == 8)
                {
                    var c1 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
                        vertices.Add(c1);
                        vertices.Add(p1);
                        vertices.Add(p2);

                        triangles.Add(verticesCount);
                        triangles.Add(verticesCount + 2);
                        triangles.Add(verticesCount + 1);
                    }
                }
                else if (cubeIndex == 9)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p3 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);
                    var p4 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                        AddEdgesPairToList(p3, p4);
                    }
                    if (generateMesh)
                    {
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
                }
                else if (cubeIndex == 10)
                {
                    var c1 = cornerLocations[1];
                    var c2 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
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
                }
                else if (cubeIndex == 11)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[1];
                    var c3 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 2, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
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
                }
                else if (cubeIndex == 12)
                {
                    var c1 = cornerLocations[2];
                    var c2 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
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
                }
                else if (cubeIndex == 13)
                {
                    var c1 = cornerLocations[0];
                    var c2 = cornerLocations[2];
                    var c3 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 1, 3);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
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
                }
                else if (cubeIndex == 14)
                {
                    var c1 = cornerLocations[1];
                    var c2 = cornerLocations[2];
                    var c3 = cornerLocations[3];
                    var p1 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 1);
                    var p2 = GetPointAlongEdge(cornerLocations, cornerValues, 0, 2);

                    if (generateCollisions)
                    {
                        AddEdgesPairToList(p1, p2);
                    }
                    if (generateMesh)
                    {
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
                #endregion
            }
        }

        if (generateMesh)
        {
            mesh.Optimize();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            meshDirty = false;
        }

        if (generateCollisions)
        {
            linkedEdgesList = new HashSet<LinkedList<Vector2>>();
            var foundEdgePairs = new HashSet<LinkedList<Vector2>>();

            var initialLinkedList = new LinkedList<Vector2>();
            foreach (var edgePair in edgePairsList)
            {
                initialLinkedList.AddLast(edgePair.First.Value);
                initialLinkedList.AddLast(edgePair.Last.Value);
                foundEdgePairs.Add(edgePair);
                break;
            }
            linkedEdgesList.Add(initialLinkedList);

            while (foundEdgePairs.Count != edgePairsList.Count)
            {
                bool linkedPair = false;

                foreach (var linkedEdges in linkedEdgesList)
                {
                    foreach (var edgePair in edgePairsList)
                    {
                        if (foundEdgePairs.Contains(edgePair)) continue;

                        var firstMatch = edgePair.First.Value == linkedEdges.Last.Value;
                        var lastMatch = edgePair.Last.Value == linkedEdges.Last.Value;

                        if (firstMatch || lastMatch)
                        {
                            if (firstMatch)
                            {
                                linkedEdges.AddLast(edgePair.Last.Value);
                            }
                            else
                            {
                                linkedEdges.AddLast(edgePair.First.Value);
                            }

                            linkedPair = true;
                            foundEdgePairs.Add(edgePair);
                        }
                    }
                }

                if (!linkedPair)
                {
                    var newLinkedList = new LinkedList<Vector2>();
                    foreach (var edgePair in edgePairsList)
                    {
                        if (foundEdgePairs.Contains(edgePair)) continue;

                        newLinkedList.AddLast(edgePair.First.Value);
                        newLinkedList.AddLast(edgePair.Last.Value);
                        foundEdgePairs.Add(edgePair);
                        break;
                    }
                    linkedEdgesList.Add(newLinkedList);
                }
            }

            collisionsDirty = false;
        }
    }

    private int GetCornerIndex(int x, int y)
    {
        return x + y * MarchingSquaresManager.DataWidth;
    }

    private int GetSquareIndex(float corner0, float corner1, float corner2, float corner3)
    {
        var index = 0;

        if (corner0 >= MarchingSquaresManager.IsoLevel) index |= 1;
        if (corner1 >= MarchingSquaresManager.IsoLevel) index |= 2;
        if (corner2 >= MarchingSquaresManager.IsoLevel) index |= 4;
        if (corner3 >= MarchingSquaresManager.IsoLevel) index |= 8;

        return index;
    }

    private Vector3 GetPointAlongEdge(Vector3Int[] p, float[] v, int i1, int i2)
    {
        var p1 = p[i1];
        var p2 = p[i2];
        var v1 = v[i1];
        var v2 = v[i2];

        var mul = (MarchingSquaresManager.IsoLevel - v1) / (v2 - v1);

        return Vector3.Lerp(p1, p2, mul);
    }
}
