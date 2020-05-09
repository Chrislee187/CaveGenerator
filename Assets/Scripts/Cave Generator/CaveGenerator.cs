using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

[ExecuteInEditMode]
public class CaveGenerator : MonoBehaviour
{
    [Header("Map size settings")]
    [Tooltip("Changing the map size will effectively generate a new random map, even when using " +
             "the same seed. The seed is honoured and the same sequence of random numbers will be generated but they are now in different places " +
             "on the newly sized map")]
    [Min(1)]
    public int Width = 128;

    [Tooltip("Changing the map size will effectively generate a new random map, even when using " +
             "the same seed. The seed is honoured and the same sequence of random numbers will be generated but they are now in different places " +
             "on the newly sized map")]
    [Min(1)]
    public int Height = 128;

    [Header("Randomisation settings")]
    [Tooltip("When the random number (between 0-100) in floor plan creation is LESS THAN this value a wall will be placed, otherwise the cell is empty.")]
    [Range(0 ,100)]
    public int InitialMapFillPercent = 50;

    [Tooltip("Seed to use to initialise the random number generator")]
    public string Seed = "";

    [Tooltip("Use a random seed each time the map is generated")]
    public bool UseRandomSeed;

    [Header("Smoothing and Border settings")]
    [Tooltip("Size in squares of the border to add around the edge of the floor map. NB. This is added to the specified width and height of the map and is in addition to the single square wall that " +
             "is automatically inserted around the inner border of the map to ensure small wall/room thresholds work in correctly around the edges of the map.")]
    [Min(0)]
    public int BorderSize = 1;

    [Tooltip("Number of times to apply the Smoothing algorithm on the initial randomised map data.")]
    [Range(0,25)]
    public int SmoothingIterations = 5;

    [Header("Mesh Settings")]
    [Tooltip("Generate the inverse floor plan mesh (inverse because we are generating the floor plan by generating the walls, floor is the areas NOT covered by walls")]
    public bool GenerateFloorPlanMesh = true;
    [Tooltip("Generate a vertical mesh for the walls, to use the cave in 3D")]
    public bool GenerateWallMesh = true;
    
    
    [Header("Region Settings")]
    [Tooltip("Use to remove small wall 'islands' and small rooms")]
    public bool ProcessRegions = true;

    [Min(0)]
    [Tooltip("When processing regions, remove walls with less than this number of nodes. This removes any small 'islands' in the middle or larger open spaces")]
    public int SmallWallThresholdSize = 25;

    [Min(0)]
    [Tooltip("When processing regions, remove and wall outlines with less than this number of nodes. This removes any small rooms.")]
    public int SmallRoomThresholdSize = 25;

    
    private string _lastSeed = "";

    private void Start()
    {
#if UNITY_EDITOR
        GenerateCave();
#endif
    }

    [ContextMenu("Generate New Cave")]
    
    public void GenerateCave()
    {

        var floorPlanGenerator = new CaveFloorPlanGenerator(Width, Height);

        var map = UseRandomSeed
            ? floorPlanGenerator.Generate(InitialMapFillPercent, BorderSize, SmoothingIterations, ProcessRegions, SmallWallThresholdSize, SmallRoomThresholdSize)
            : floorPlanGenerator.Generate(Seed, InitialMapFillPercent, BorderSize, SmoothingIterations, ProcessRegions, SmallWallThresholdSize, SmallRoomThresholdSize);

        Seed = floorPlanGenerator.Seed;
        _lastSeed = Seed;
        
        var meshGenerator = GetComponent<MeshGenerator>();
        meshGenerator.GenerateMesh(map, 1f, GenerateFloorPlanMesh, GenerateWallMesh);

    }

#if UNITY_EDITOR
    [ContextMenu("Map Generation Performance Test")]
    public void PerfTest()
    {
        var sw = new Stopwatch();
        var avgs = new List<long>();
        for (var i = 0; i < 10; i++)
        {
            sw.Start();

            GenerateCave();

            sw.Stop();
            avgs.Add(sw.ElapsedMilliseconds);
            sw.Reset();
        }
        Debug.Log($"10 Generations took an average of {avgs.Average()}ms");
    }

    private void OnValidate()
    {
        if (GUI.changed)
        {
            if (Seed == _lastSeed && UseRandomSeed)
            {
                UseRandomSeed = false;
                GenerateCave();
                UseRandomSeed = true;
            }
            else
            {
                GenerateCave();
            }
        }

    }
#endif
}
