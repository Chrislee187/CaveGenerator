using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public MeshFilter Walls;

    public void GenerateMesh(int[,] map, float squareSize, 
        bool generateFloorPlan = true,
        bool generateWalls = true
        )
    {
        Walls.sharedMesh.Clear();
        GetComponent<MeshFilter>().sharedMesh.Clear();

        var marchingSquares = new MarchingSquaresMeshData(map, squareSize);

        if (generateFloorPlan)
        {
            var floorplan = new FloorPlanMeshCreator(marchingSquares);
            GetComponent<MeshFilter>().mesh = floorplan.Create();
        }

        if (generateWalls)
        {
            Walls.mesh = new WallMeshCreator(marchingSquares).Create();
        }
    }

}
