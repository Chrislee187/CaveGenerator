using System;
using System.Collections.Generic;

public partial class CaveMapGenerator
{
    public partial class Regions
    {
        public class Room : Region, IComparable<Room>
        {
            public readonly List<Coord> EdgeTiles;
            public bool IsMainRoom { get; set; }
            public bool IsAccessibleFromMainRoom { get; set; }
            public bool HasAConnection => _connectedRooms.Count > 0;
            public void AddConnection(Room r) => _connectedRooms.Add(r);
            private readonly List<Room> _connectedRooms;
            private readonly int _roomSize;

            public Room() : base(true)
            {
                
            }

            public Room(List<Coord> roomTiles,int [,] map ) : base(true)
            {
                AddRange(roomTiles);
                
                _roomSize = roomTiles.Count;
                _connectedRooms = new List<Room>();
                    
                EdgeTiles = new List<Coord>();

                foreach (var tile in this)
                {
                    for (var x = tile.TileX-1; x <= tile.TileX+1; x++)
                    {
                        for (var y = tile.TileY-1; y <= tile.TileY + 1; y++)
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

            public void SetAccessibleFromMainRoom()
            {
                if (!IsAccessibleFromMainRoom)
                {
                    IsAccessibleFromMainRoom = true;
                    foreach (var connectedRoom in _connectedRooms)
                    {
                        connectedRoom.SetAccessibleFromMainRoom();
                    }
                }
            }

            public static void ConnectRooms(Room a, Room b)
            {
                if (a.IsAccessibleFromMainRoom)
                {
                    b.SetAccessibleFromMainRoom();
                }
                else if (b.IsAccessibleFromMainRoom)
                {
                    a.SetAccessibleFromMainRoom();
                }

                a.AddConnection(b);
                b.AddConnection(a);
            }

            public bool IsConnected(Room otherRoom) => _connectedRooms.Contains(otherRoom);

            public int CompareTo(Room other) => other._roomSize.CompareTo(_roomSize);
        }
    }
}