using UnityEngine;

public interface IMeshCreator
{
    Mesh Create();
}

public class FloorPlanMeshCreator : IMeshCreator
{
    private readonly MarchingSquaresMeshData _marchingSquaresMeshData;

    public FloorPlanMeshCreator(MarchingSquaresMeshData marchingSquaresMeshData)
    {
        _marchingSquaresMeshData = marchingSquaresMeshData;
    }

    public Mesh Create()
    {
        var mesh = new Mesh
        {
            vertices = _marchingSquaresMeshData.Vertices,
            triangles = _marchingSquaresMeshData.Triangles
        };
        mesh.RecalculateNormals();
        return mesh;
    }
}