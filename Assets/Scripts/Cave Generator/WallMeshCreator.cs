using System.Collections.Generic;
using UnityEngine;

public class WallMeshCreator
{
    private readonly MarchingSquaresMeshData _marchingSquaresMeshData;
    private readonly List<List<int>> _outlines = new List<List<int>>();
    public WallMeshCreator(MarchingSquaresMeshData marchingSquaresMeshData)
    {
        _marchingSquaresMeshData = marchingSquaresMeshData;
    }

    public Mesh Create()
    {
        CalculateMeshOutlines();
        var wallVertices = new List<Vector3>();
        var wallTriangles = new List<int>();
        var wallMesh = new Mesh();
        var wallHeight = 5;

        const int topLeftOffset = 0;
        const int topRightOffset = 1;
        const int bottomLeftOffset = 2;
        const int bottomRightOffset = 3;

        foreach (var outline in _outlines)
        {
            for (var i = 0; i < outline.Count - 1; i++)
            {
                var startIndex = wallVertices.Count;
                wallVertices.Add(_marchingSquaresMeshData.Vertices[outline[i]]); // top left Vertex
                wallVertices.Add(_marchingSquaresMeshData.Vertices[outline[i + 1]]); // top right Vertex
                wallVertices.Add(_marchingSquaresMeshData.Vertices[outline[i]] - Vector3.up * wallHeight); // bottom left Vertex
                wallVertices.Add(_marchingSquaresMeshData.Vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right Vertex

                // Build the triangles anti-clockwise so the wall is facing the inside
                wallTriangles.Add(startIndex + topLeftOffset);
                wallTriangles.Add(startIndex + bottomLeftOffset);
                wallTriangles.Add(startIndex + bottomRightOffset);
                
                wallTriangles.Add(startIndex + bottomRightOffset);
                wallTriangles.Add(startIndex + topRightOffset);
                wallTriangles.Add(startIndex + topLeftOffset);
            }
        }

        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.RecalculateNormals();
        return wallMesh;

    }

    private void CalculateMeshOutlines()
    {
        for (var vertexIndex = 0; vertexIndex < _marchingSquaresMeshData.Vertices.Length; vertexIndex++)
        {
            if (!_marchingSquaresMeshData.CheckedVertices.Contains(vertexIndex))
            {
                var newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);

                if (newOutlineVertex != -1)
                {
                    _marchingSquaresMeshData.CheckedVertices.Add(vertexIndex);

                    var newOutline = new List<int> { vertexIndex };

                    _outlines.Add(newOutline);

                    FollowOutline(newOutlineVertex, _outlines.Count - 1);

                    // Connect to starting vertex
                    _outlines[_outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    private void FollowOutline(int vertexIndex, int outlineIndex)
    {
        while (true)
        {
            _outlines[outlineIndex].Add(vertexIndex);
            _marchingSquaresMeshData.CheckedVertices.Add(vertexIndex);

            var nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
            if (nextVertexIndex == -1)
            {
                break;
            }

            vertexIndex = nextVertexIndex;
        }
    }

    private int GetConnectedOutlineVertex(int vertexIndex)
    {
        var trianglesWithVertex = _marchingSquaresMeshData.TriangleDictionary[vertexIndex];

        for (var i = 0; i < trianglesWithVertex.Count; i++)
        {
            var triangle = trianglesWithVertex[i];

            for (var j = 0; j < 3; j++)
            {
                var vertexB = triangle[j];

                if (IsOutlineEdge(vertexIndex, vertexB) && vertexIndex != vertexB && !_marchingSquaresMeshData.CheckedVertices.Contains(vertexB))
                {
                    return vertexB;
                }
            }
        }

        return -1;
    }

    private bool IsOutlineEdge(int vertexA, int vertexB)
    {
        var trianglesWithVertexA = _marchingSquaresMeshData.TriangleDictionary[vertexA];

        var sharedTriangleCount = 0;

        for (var i = 0; i < trianglesWithVertexA.Count; i++)
        {
            if (trianglesWithVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                    return false;
            }
        }

        return sharedTriangleCount == 1;

    }

}