using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public void GenerateMesh(int[,] map, float squareSize)
    {
        var marchingSquares = new MarchingSquaresMesh(map, squareSize);

        var mesh = new Mesh();
        GetComponent<MeshFilter>()
            .mesh = mesh;

        mesh.vertices = marchingSquares.Vertices;
        mesh.triangles = marchingSquares.Triangles;
        mesh.RecalculateNormals();
    }
}
