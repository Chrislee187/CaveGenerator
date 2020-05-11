﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class CaveFloorPlanGenerator
{
    public const int AWall = 1;
    public const int NoWall = 0;

    public string Seed;

    private readonly int _nonBorderedWidth;
    private readonly int _nonBorderedHeight;
    private int _borderSize;

    private readonly bool _sealMap = true;

    public CaveFloorPlanGenerator(int nonBorderedWidth, int nonBorderedHeight)
    {
        _nonBorderedWidth = nonBorderedWidth;
        _nonBorderedHeight = nonBorderedHeight;
    }
    
    public int[,] Generate(int randomFillPercent = 50, int borderSize = 1, int smoothingIterations = 5, bool processRegions = true, int smallWallThresholdSize = 50, int smallRoomThresholdSize = 50)
        => Generate(Guid.NewGuid().ToString(),
            randomFillPercent,
            borderSize, 
            smoothingIterations,
            processRegions,
            smallWallThresholdSize,
            smallRoomThresholdSize);

    public int[,] Generate(string seed, int randomFillPercent,int borderSize = 1, int smoothingIterations = 5, bool processRegions = true, int smallWallThresholdSize = 50, int smallRoomThresholdSize = 50)
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

        if (processRegions)
        {
            var regions = new Regions(map);
            regions.RemoveSmallWalls(smallWallThresholdSize);
            regions.RemoveSmallRooms(smallRoomThresholdSize);
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

    private class Regions : List<Regions.Region>
    {
        private readonly int[,] _map;
        private readonly int _mapWidth;
        private readonly int _mapHeight;

        public Regions(int[,] map)
        {
            _map = map;
            _mapWidth = _map.GetUpperBound(0);
            _mapHeight = _map.GetUpperBound(1);
        }
        private bool IsInGridBounds(int x, int y) => x >= 0 && x < _mapWidth && y >= 0 && y < _mapHeight;
        public void RemoveSmallWalls(int smallWallThresholdSize) => RemoveRegions(_map, AWall, NoWall, smallWallThresholdSize);
        public void RemoveSmallRooms(int smallRoomThresholdSize) => RemoveRegions(_map, NoWall, AWall, smallRoomThresholdSize);
        
        private IReadOnlyList<Region> GetRegions(int[,] map, int tileType)
        {
            var regions = new List<Region>();
            var checkedTiles = new bool[_mapWidth, _mapHeight];

            for (var x = 0; x < _mapWidth; x++)
            {
                for (var y = 0; y < _mapHeight; y++)
                {
                    if (!checkedTiles[x, y] && map[x, y] == tileType)
                    {
                        var newRegion = GetRegionTiles(x, y, map);
                        regions.Add(newRegion);

                        foreach (var tile in newRegion)
                        {
                            checkedTiles[tile.TileX, tile.TileY] = true;
                        }
                    }
                }

            }

            return regions;
        }

        private Region GetRegionTiles(int startX, int startY, int[,] map)
        {

            var tiles = new Region();
            var checkedTiles = new bool[_mapWidth, _mapHeight];
            var tileType = map[startX, startY];

            var queue = new Queue<Coord>();

            queue.Enqueue(new Coord(startX, startY));
            checkedTiles[startX, startY] = true;

            while (queue.Count > 0)
            {
                var tile = queue.Dequeue();
                tiles.Add(tile);

                for (var x = tile.TileX - 1; x <= tile.TileX + 1; x++)
                {
                    for (var y = tile.TileY - 1; y <= tile.TileY + 1; y++)
                    {
                        if (IsInGridBounds(x, y) && (x == tile.TileX || y == tile.TileY))
                        {
                            if (!checkedTiles[x, y] && map[x, y] == tileType)
                            {
                                checkedTiles[x, y] = true;
                                queue.Enqueue(new Coord(x, y));
                            }
                        }
                    }
                }
            }

            return tiles;
        }
        
        private void RemoveRegions(int[,] map, int tileType, int replacementTileType, int threshold)
        {
            var regions = GetRegions(map, tileType);
            List<Room> remainingRooms = new List<Room>();
            foreach (var region in regions)
            {
                if (region.Count < threshold)
                {
                    foreach (var tile in region)
                    {
                        map[tile.TileX, tile.TileY] = replacementTileType;
                    }
                }
                else
                {
                    if (tileType == NoWall)
                    {
                        remainingRooms.Add(new Room(region, map));
                    }
                }
            }

            if (tileType == NoWall)
            {
                ConnectClosestRooms(remainingRooms);
            }
        }

        void ConnectClosestRooms(List<Room> allRooms)
        {
            var closest = 0;
            Coord closestTileA = new Coord();
            Coord closestTileB = new Coord();
            Room closestRoomA = new Room();
            Room closestRoomB = new Room();
            var possibleConnectionFound = false;

            foreach (var roomA in allRooms)
            {
                possibleConnectionFound = false;
                foreach (var roomB in allRooms.Where(roomB => !roomA.Equals(roomB)))
                {
                    if (roomA.IsConnected(roomB))
                    {
                        possibleConnectionFound = false;
                        break;
                    }

                    foreach (var tileA in roomA.EdgeTiles)
                    {
                        foreach (var tileB in roomB.EdgeTiles)
                        {
                            var distanceBetweenRooms = (int) (Mathf.Pow(tileA.TileX - tileB.TileX, 2)
                                                              + Mathf.Pow(tileA.TileY - tileB.TileY, 2));

                            if (distanceBetweenRooms < closest || !possibleConnectionFound)
                            {
                                closest = distanceBetweenRooms;
                                possibleConnectionFound = true;
                                closestTileA = tileA;
                                closestTileB = tileB;
                                closestRoomA = roomA;
                                closestRoomB = roomB;

                            }
                        }
                    }
                }

                if (possibleConnectionFound)
                {
                    CreatePassage(closestRoomA, closestRoomB, closestTileA, closestTileB);
                }
            }

        }

        void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
        {
            Room.ConnectRooms(roomA, roomB);

            Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 5);

        }

        Vector3 CoordToWorldPoint(Coord tile)
        {
            return new Vector3(-_mapWidth / 2 + .5f + tile.TileX, 2, -_mapHeight / 2 + .5f + tile.TileY);
        }
        internal class Region : List<Coord>
        {

        }
        internal readonly struct Coord
        {
            public readonly int TileX;
            public readonly int TileY;

            public Coord(int tileX, int tileY)
            {
                TileX = tileX;
                TileY = tileY;
            }
        }

        private class Room
        {
            public List<Coord> Tiles;
            public List<Coord> EdgeTiles;
            public List<Room> ConnectedRooms;
            public int RoomSize;

            public Room()
            {
                
            }

            public Room(List<Coord> roomTiles,int [,] map )
            {
                Tiles = roomTiles;
                RoomSize = Tiles.Count;
                ConnectedRooms = new List<Room>();
                    
                EdgeTiles = new List<Coord>();

                foreach (var tile in Tiles)
                {
                    for (int x = tile.TileX-1; x <= tile.TileX+1; x++)
                    {
                        for (int y = tile.TileY-1; y <= tile.TileY + 1; y++)
                        {
                            if (x == tile.TileX || y == tile.TileY) // ignore diaganoals
                            {
                                if (map[x, y] == AWall)
                                {
                                    EdgeTiles.Add(tile);
                                }
                            }
                        }
                    }
                }




            }

            public static void ConnectRooms(Room a, Room b)
            {
                a.ConnectedRooms.Add(b);
                b.ConnectedRooms.Add(a);
            }

            public bool IsConnected(Room otherRoom)
            {
                return ConnectedRooms.Contains(otherRoom);
            }
        }
    }
}