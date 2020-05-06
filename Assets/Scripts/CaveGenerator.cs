using UnityEngine;


[ExecuteInEditMode]
public class CaveGenerator : MonoBehaviour
{
    [Min(1)]
    public int Width = 128;
    [Min(1)]
    public int Height = 64;

    [Tooltip("When the random number in floor plan creation is LESS THAN this percentage a wall will be placed, otherwise the cell is empty.")]
    [Range(0 ,100)]
    public int InitialMapFillPercent = 50;

    [Tooltip("Seed to use to initialise the random number generator")]
    public string Seed = "";

    [Tooltip("Use a random seed each time the map is generated")]
    public bool UseRandomSeed;

    [Min(0)]
    public int BorderSize = 1;

    private CaveFloorPlanGenerator _floorPlanGenerator;
    private int[,] _map;

    void Start()
    {
        GenerateCave();
    }

    private string lastSeed = "";
    public void OnValidate()
    {
        if (GUI.changed)
        {
            if (Seed == lastSeed && UseRandomSeed)
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

    public void GenerateCave()
    {
        _floorPlanGenerator = new CaveFloorPlanGenerator(Width, Height);

        if (UseRandomSeed)
        {
            _map = _floorPlanGenerator.GenerateRandom(InitialMapFillPercent, BorderSize);
        }
        else
        {
            _map = this._floorPlanGenerator.Generate(Seed, InitialMapFillPercent, BorderSize);
        }
        Seed = _floorPlanGenerator.Seed;
        lastSeed = Seed;
    }

    void OnDrawGizmos()
    {
        if(_map != null)
        {
            var xOffset = -Width / 2f + 0.5f;
            var zOffset = -Height / 2f + 0.5f;
            For.Xy(Width, Height, (x, y) =>
            {
                Gizmos.color = _map[x, y] == 1 ? Color.blue : Color.white;
                var pos = new Vector3(xOffset + x, 0, zOffset + y);
                Gizmos.DrawCube(pos, Vector3.one);
            });
        }
    }
}
