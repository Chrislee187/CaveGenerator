
using System;
using Random = System.Random;

public class CaveFloorPlanGenerator
{
    public const int AWall = 1;
    public const int NoWall = 0;

    public string Seed;

    private readonly int _nonBorderedWidth;
    private readonly int _nonBorderedHeight;
    private int _borderSize;


    public CaveFloorPlanGenerator(int nonBorderedWidth, int nonBorderedHeight)
    {
        _nonBorderedWidth = nonBorderedWidth;
        _nonBorderedHeight = nonBorderedHeight;
    }
    
    public int[,] GenerateRandom(int randomFillPercent = 50, int borderSize = 1, int smoothingIterations = 5)
        => Generate(Guid.NewGuid().ToString(),
            randomFillPercent,
            borderSize, 
            smoothingIterations);

    public int[,] Generate(string seed, int randomFillPercent,int borderSize = 1, int smoothingIterations = 5)
    {
        Seed = seed;

        _borderSize = borderSize;

        var map = new int[_nonBorderedWidth, _nonBorderedHeight];

        RandomiseMap(map, Seed, randomFillPercent);

        if (smoothingIterations > 0)
        {
            for (var i = 0; i < smoothingIterations; i++)
            {
                ApplySmoothing(map);
            }
        }

        return AddBorderToMap(borderSize, map);

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
            map[x, y] = random.Next(0, 100) < randomFillPercent
                ? AWall
                : NoWall;
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
        bool IsInGridBounds(int x, int y) => x >= 0 && x < _nonBorderedWidth && y >= 0 && y < _nonBorderedHeight;

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
    
}