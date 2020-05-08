using UnityEngine;

public class FloorPlanMeshCreator
{
    private readonly MarchingSquares _marchingSquares;

    public FloorPlanMeshCreator(MarchingSquares marchingSquares)
    {
        _marchingSquares = marchingSquares;
    }

    public Mesh Create()
    {
        var mesh = new Mesh
        {
            vertices = _marchingSquares.Vertices,
            triangles = _marchingSquares.Triangles
        };
        mesh.RecalculateNormals();
        return mesh;
    }
}