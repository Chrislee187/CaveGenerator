using System;
using System.Collections.Generic;
using UnityEngine;

public partial class CaveMapGenerator
{
    /// <summary>
    /// Given a map determine all the room and non-room (wall) areas
    /// </summary>
    public partial class Regions : List<Regions.Region>
    {
        public IEnumerable<Room> Rooms => _rooms;

        private readonly int[,] _map;
        private readonly int _mapWidth;
        private readonly int _mapHeight;

        private List<Room> _rooms;
        private readonly bool _connectAllRooms;
        private readonly int _passageRadius;

        public Regions(int[,] map, int smallWallThresholdSize, int smallRoomThresholdSize,
            bool connectAllRooms = true, int passageRadius = 1)
        {
            _passageRadius = passageRadius;
            _connectAllRooms = connectAllRooms;
            _map = map;
            _mapWidth = _map.GetUpperBound(0);
            _mapHeight = _map.GetUpperBound(1);
            _rooms = new List<Room>();

            RemoveSmallWalls(smallWallThresholdSize);
            FillInSmallRooms(smallRoomThresholdSize);
        }
        private bool IsInGridBounds(int x, int y) => x >= 0 && x < _mapWidth && y >= 0 && y < _mapHeight;
        
        private void RemoveSmallWalls(int smallWallThresholdSize) 
            => ReplaceRegionsBelowThreshold(_map, AWall, NoWall, smallWallThresholdSize);
        
        private void FillInSmallRooms(int smallRoomThresholdSize)
        {
            var remainingRooms = new List<Room>();

            ReplaceRegionsBelowThreshold(_map, NoWall, AWall, 
                smallRoomThresholdSize,
                (region) =>
                {
                    if (_connectAllRooms)
                    {
                        var newRoom = new Room(region, _map);
                        remainingRooms.Add(newRoom);
                    }
                });

            if (_connectAllRooms)
            {
                remainingRooms.Sort();
                remainingRooms[0].IsMainRoom = true;
                remainingRooms[0].IsAccessibleFromMainRoom = true;
                ConnectClosestRooms(remainingRooms);
            }

            _rooms = remainingRooms;
        }

        private IReadOnlyList<Region> GetRegions(int[,] map, int tileType)
        {
            var regions = new List<Region>();
            var checkedTiles = new bool[_mapWidth, _mapHeight];

            For.Xy(_mapWidth, _mapHeight, (x, y) =>
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

            });

            return regions;
        }

        private Region GetRegionTiles(int startX, int startY, int[,] map)
        {

            var checkedTiles = new bool[_mapWidth, _mapHeight];
            var tileType = map[startX, startY];
            var tiles = new Region(tileType == NoWall);

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

        private void ReplaceRegionsBelowThreshold(int[,] map,
            int tileType, int replacementTileType,
            int threshold)
            => ReplaceRegionsBelowThreshold(map,
                tileType, replacementTileType,
                threshold,
                (r) => { });

        private void ReplaceRegionsBelowThreshold(int[,] map, 
            int tileType, int replacementTileType,
            int threshold,
            Action<Region> aboveThresholdRegion)
        {
            var regions = GetRegions(map, tileType);

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
                    aboveThresholdRegion(region);
                }
            }
        }

        private void ConnectClosestRooms(List<Room> allRooms, bool ensureConnectionToMainRoom = false)
        {
            var roomListA = new List<Room>();
            var roomListB = new List<Room>();

            if (ensureConnectionToMainRoom)
            {
                foreach (var room in allRooms)
                {
                    if (room.IsAccessibleFromMainRoom)
                    {
                        roomListB.Add(room);
                    }
                    else
                    {
                        roomListA.Add(room);
                    }
                }
            }
            else
            {
                roomListA = allRooms;
                roomListB = allRooms;
            }

            var passage = new Passageway(_passageRadius, _map)
            {
                Distance = 0,
                StartTile = new Coord(),
                EndTile = new Coord(),
                StartRoom = new Room(),
                EndRoom = new Room()
            };

            var possibleConnectionFound = false;

            foreach (var roomA in roomListA)
            {
                if (!ensureConnectionToMainRoom)
                {
                    possibleConnectionFound = false;
                    if (roomA.HasAConnection)
                    {
                        continue;
                    }
                }


                foreach (var roomB in roomListB)
                {
                    if (roomA == roomB || roomA.IsConnected(roomB))
                    {
                        continue;
                    }

                    foreach (var tileA in roomA.EdgeTiles)
                    {
                        foreach (var tileB in roomB.EdgeTiles)
                        {
                            var distanceBetweenRooms = (int) (Mathf.Pow(tileA.TileX - tileB.TileX, 2)
                                                              + Mathf.Pow(tileA.TileY - tileB.TileY, 2));

                            if (distanceBetweenRooms < passage.Distance || !possibleConnectionFound)
                            {
                                passage = new Passageway(_passageRadius, _map)
                                {
                                    Distance = distanceBetweenRooms,
                                    StartTile = tileA,
                                    EndTile = tileB,
                                    StartRoom = roomA,
                                    EndRoom = roomB
                                };
                                possibleConnectionFound = true;

                            }
                        }
                    }
                }

                if (possibleConnectionFound && !ensureConnectionToMainRoom)
                {
                    passage.Tunnel();
                }
            }


            if (possibleConnectionFound && ensureConnectionToMainRoom)
            {
                passage.Tunnel();
                ConnectClosestRooms(allRooms, true);
            }
            if (!ensureConnectionToMainRoom)
            {
                ConnectClosestRooms(allRooms, true);
            }

        }


        public Vector3 CoordToWorldPoint(Coord tile) 
            => new Vector3(-_mapWidth / 2f + .5f + tile.TileX, 2, -_mapHeight / 2f + .5f + tile.TileY);
    }
}