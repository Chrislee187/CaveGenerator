using System.Linq;
using UnityEngine;
using Random = System.Random;

public partial class CaveMapGenerator
{
    public const int AWall = 1;
    public const int NoWall = 0;

    public Regions Areas { get; private set; }

    public string Seed;

    private readonly int _nonBorderedWidth;
    private readonly int _nonBorderedHeight;
    private int _borderSize;

    private readonly bool _sealMap = true;

    public CaveMapGenerator(int nonBorderedWidth, int nonBorderedHeight)
    {
        _nonBorderedWidth = nonBorderedWidth;
        _nonBorderedHeight = nonBorderedHeight;
    }


    // TODO: Refactor so Map is a first class concept, with width, hegiht and InBounds() support
    public int[,] Generate(CaveSettings caveSettings)
    {
        Seed = caveSettings.Seed;

        _borderSize = caveSettings.BorderSize;

        var map = new int[_nonBorderedWidth, _nonBorderedHeight];

        RandomiseMap(map, Seed, caveSettings.RandomFillPercent);

        if (caveSettings.SmoothingIterations > 0)
        {
            for (var i = 0; i < caveSettings.SmoothingIterations; i++)
            {
                ApplySmoothing(map);
            }
        }

        if (caveSettings.ProcessRegions)
        {
            Areas = new Regions(map, 
                caveSettings.SmallWallThresholdSize, caveSettings.SmallRoomThresholdSize, 
                caveSettings.EnsureAllRoomsConnected, caveSettings.InterconnectingPassageWidth);

            // HighlightRooms();
        }

        return AddBorderToMap(caveSettings.BorderSize, map);
    }

    private void HighlightRooms()
    {
        foreach (var room in Areas.Rooms)
        {
            foreach (var currentTile in room.EdgeTiles)
            {
                var currentPoint = Areas.CoordToWorldPoint(currentTile);
                Debug.DrawLine(currentPoint, currentPoint + Vector3.up * 5,
                    Color.black, 5);
            }
        }
    }

    private int[,] AddBorderToMap(int borderSize, int[,] map)
    {
        if (_borderSize == 0) return map;

        bool IsBorder(int x, int y) => x <= _borderSize || x >= _nonBorderedWidth + _borderSize || y <= _borderSize || y >= _nonBorderedHeight + _borderSize;

        var borderedMap = new int[_nonBorderedWidth + _borderSize * 2, _nonBorderedHeight + _borderSize * 2];

        For.Xy(borderedMap.GetLength(0), borderedMap.GetLength(1), (x, y) =>
        {
            if (IsBorder(x, y))
            {
                borderedMap[x, y] = AWall;
            }
            else
            {
                borderedMap[x, y] = map[x - borderSize, y - borderSize];
            }
        });
        return borderedMap;
    }

    private void RandomiseMap(int[,] map, string seed, int randomFillPercent)
    {
        var random = new Random(seed.GetHashCode());
        For.Xy(_nonBorderedWidth, _nonBorderedHeight, (x, y) =>
        {
            if (_sealMap && x == 0 || x == _nonBorderedWidth - 1 || y == 0 || y == _nonBorderedHeight - 1)
            {
                map[x, y] = AWall;
            }
            else
            {
                map[x, y] = random.Next(0, 100) < randomFillPercent
                    ? AWall
                    : NoWall;
            }
        });
    }
    
    private void ApplySmoothing(int[,] map)
    {
        For.Xy(_nonBorderedWidth, _nonBorderedHeight, (x, y) =>
        {
            var neighbourWalls = NineNeighbourWallCount(x, y, map);
            if (neighbourWalls > 4)
            {
                map[x, y] = AWall;
            }
            else if (neighbourWalls < 4)
            {
                map[x, y] = NoWall;
            }
        });
    }

    /// <summary>
    /// Get the eight adjacent cells (known as the <see href="https://en.wikipedia.org/wiki/Moore_neighborhood">"Moore neighbourhood"</see> or "nine-neighbour-square")
    /// </summary>
    /// <param name="wallX"></param>
    /// <param name="wallY"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    private int NineNeighbourWallCount(int wallX, int wallY, int[,] map)
    {
        var wallCount = 0;

        for (var neighbourX = wallX - 1; neighbourX <= wallX + 1; neighbourX++)
        {
            for (var neighbourY = wallY - 1; neighbourY <= wallY + 1; neighbourY++)
            {
                if (IsInGridBounds(neighbourX, neighbourY))
                {
                    if (neighbourX != wallX || neighbourY != wallY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    // Treat out of bounds as walls
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    private bool IsInGridBounds(int x, int y) => x >= 0 && x < _nonBorderedWidth && y >= 0 && y < _nonBorderedHeight;

}