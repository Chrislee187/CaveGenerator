using UnityEngine;


[ExecuteInEditMode]
public class CaveGenerator : MonoBehaviour
{
    [Tooltip("Changing the map size will effectively generate a new random map, even when using " +
             "the same seed. The seed is honoured and the same sequence of random numbers will be generated but they are now in different places " +
             "on the newly sized map")]
    [Min(1)]
    public int Width = 128;

    [Tooltip("Changing the map size will effectively generate a new random map, even when using " +
             "the same seed. The seed is honoured and the same sequence of random numbers will be generated but they are now in different places " +
             "on the newly sized map")]
    [Min(1)]
    public int Height = 64;

    [Tooltip("When the random number (between 0-100) in floor plan creation is LESS THAN this value a wall will be placed, otherwise the cell is empty.")]
    [Range(0 ,100)]
    public int InitialMapFillPercent = 50;

    [Tooltip("Seed to use to initialise the random number generator")]
    public string Seed = "";

    [Tooltip("Use a random seed each time the map is generated")]
    public bool UseRandomSeed;

    [Tooltip("Size in squares of the border to place around the edge of the floor map. NB. Changing the border size will effectively generate a new random map " +
             "for the same reasons as when the map Width & Height changes (see their ToolTips).")]
    [Min(0)]
    public int BorderSize = 1;

    [Tooltip("Number of times to apply the Smoothing algorithm on the initial randomised map data.")]
    [Range(0,25)]
    public int SmoothingIterations = 5;


    private int[,] _map;
    private string _lastSeed = "";

    void Start()
    {
        GenerateCave();
    }

    public void GenerateCave()
    {
        var floorPlanGenerator = new CaveFloorPlanGenerator(Width, Height);

        _map = UseRandomSeed
            ? floorPlanGenerator.GenerateRandom(InitialMapFillPercent, BorderSize, SmoothingIterations)
            : floorPlanGenerator.Generate(Seed, InitialMapFillPercent, BorderSize, SmoothingIterations);

        Seed = floorPlanGenerator.Seed;
        _lastSeed = Seed;
        
        var meshGenerator = GetComponent<MeshGenerator>();
        meshGenerator.GenerateMesh(_map, 1f);
    }

#if UNITY_EDITOR
    void OnValidate()
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
