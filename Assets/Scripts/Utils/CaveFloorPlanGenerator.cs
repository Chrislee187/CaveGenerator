
using System;
using Random = System.Random;

public class CaveFloorPlanGenerator
{
    public const int AWall = 1;
    public const int NoWall = 0;

    public string Seed;

    private readonly int _width;
    private readonly int _height;
    private int _borderSize;


    public CaveFloorPlanGenerator(int width, int height)
    {
        _width = width;
        _height = height;
    }
    
    public int[,] GenerateRandom(int randomFillPercent = 50, int borderSize = 1, int smoothingIterations = 5)
        => Generate(Guid.NewGuid().ToString(),
            randomFillPercent,
            borderSize, 
            smoothingIterations);

    public int[,] Generate(string seed, int randomFillPercent,int borderSize = 1, int smoothingIterations = 5)
    {
        Seed = seed;

        this._borderSize = borderSize;

        var map = new int[_width, _height];

        RandomiseMap(map, Seed, randomFillPercent);

        if (smoothingIterations > 0)
        {
            for (var i = 0; i < smoothingIterations; i++)
            {
                ApplySmoothing(map);
            }
        }
        return map;

    }

    void RandomiseMap(int[,] map, string seed, int randomFillPercent)
    {
        var random = new Random(seed.GetHashCode());
        bool IsBorder(int x, int y)
        {
            return x < _borderSize || x > _width - _borderSize - 1
                                  || y < _borderSize || y > _height - _borderSize - 1;
        }

        For.Xy(_width, _height, (x, y) =>
        {
            if (IsBorder(x, y))
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


    void ApplySmoothing(int[,] map)
    {
        For.Xy(_width, _height, (x, y) =>
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
    int NineNeighbourWallCount(int wallX, int wallY, int[,] map)
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
                    // Our borders are solid so out of bounds cells are walls
                    wallCount++;
                }
            }
        }

        return wallCount;
    }
    private bool IsInGridBounds(int neighbourX, int neighbourY)
    {
        return neighbourX >= 0 && neighbourX < _width && neighbourY >= 0 && neighbourY < _height;
    }


}