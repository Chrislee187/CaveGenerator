using System.Collections.Generic;
using UnityEngine;

public class WallMeshCreator
{
    private readonly MarchingSquares _marchingSquares;

    public WallMeshCreator(MarchingSquares marchingSquares)
    {
        _marchingSquares = marchingSquares;
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

        foreach (var outline in _marchingSquares.Outlines)
        {
            for (var i = 0; i < outline.Count - 1; i++)
            {
                var startIndex = wallVertices.Count;
                wallVertices.Add(_marchingSquares.Vertices[outline[i]]); // top left Vertex
                wallVertices.Add(_marchingSquares.Vertices[outline[i + 1]]); // top right Vertex
                wallVertices.Add(_marchingSquares.Vertices[outline[i]] - Vector3.up * wallHeight); // bottom left Vertex
                wallVertices.Add(_marchingSquares.Vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right Vertex

                // Build the triangles anti-clockwise so the wall is facing the inside(?)
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
        for (var vertexIndex = 0; vertexIndex < _marchingSquares.Vertices.Length; vertexIndex++)
        {
            if (!_marchingSquares.CheckedVertices.Contains(vertexIndex))
            {
                var newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);

                if (newOutlineVertex != -1)
                {
                    _marchingSquares.CheckedVertices.Add(vertexIndex);

                    var newOutline = new List<int> { vertexIndex };

                    _marchingSquares.Outlines.Add(newOutline);

                    FollowOutline(newOutlineVertex, _marchingSquares.Outlines.Count - 1);

                    // Connect to starting vertex
                    _marchingSquares.Outlines[_marchingSquares.Outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    private void FollowOutline(int vertexIndex, int outlineIndex)
    {
        while (true)
        {
            _marchingSquares.Outlines[outlineIndex].Add(vertexIndex);
            _marchingSquares.CheckedVertices.Add(vertexIndex);

            var nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
            if (nextVertexIndex != -1)
            {
                vertexIndex = nextVertexIndex;
                continue;
            }

            break;
        }
    }

    private int GetConnectedOutlineVertex(int vertexIndex)
    {
        var trianglesWithVertex = _marchingSquares.TriangleDictionary[vertexIndex];

        for (var i = 0; i < trianglesWithVertex.Count; i++)
        {
            var triangle = trianglesWithVertex[i];

            for (var j = 0; j < 3; j++)
            {
                var vertexB = triangle[j];

                if (IsOutlineEdge(vertexIndex, vertexB) && vertexIndex != vertexB && !_marchingSquares.CheckedVertices.Contains(vertexB))
                {
                    return vertexB;
                }
            }
        }

        return -1;
    }

    private bool IsOutlineEdge(int vertexA, int vertexB)
    {
        var trianglesWithVertexA = _marchingSquares.TriangleDictionary[vertexA];

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