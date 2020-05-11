using System;
using System.Collections.Generic;
using UnityEngine;

public partial class CaveMapGenerator
{
    public partial class Regions
    {
        private class Passageway
        {
            private readonly int _passageRadius;
            public int Distance;
            public Coord StartTile;
            public Coord EndTile;
            public Room StartRoom;
            public Room EndRoom;

            private readonly int[,] _map;
            private readonly int _mapWidth;
            private readonly int _mapHeight;

            public Passageway(int passageRadius, int[,] map)
            {
                _passageRadius = passageRadius;
                _map = map;
                _mapWidth = map.GetUpperBound(0);
                _mapHeight = map.GetUpperBound(1);
            }
            private bool IsInGridBounds(int x, int y) => x >= 0 && x < _mapWidth && y >= 0 && y < _mapHeight;

            public void Tunnel()
            {
                Room.ConnectRooms(StartRoom, EndRoom);

                var passage = GetLine(StartTile, EndTile);

                foreach (var coord in passage)
                {
                    DrawCircle(coord, _passageRadius);
                }
            }

            void DrawCircle(Coord c, int r)
            {
                for (int x = -r; x <= r; x++)
                {
                    for (int y = -r; y <= r; y++)
                    {
                        if (x * x + y * y <= r * r)
                        {
                            int passageX = c.TileX + x;
                            int passageY = c.TileY + y;

                            if (IsInGridBounds(passageX, passageY))
                            {
                                _map[passageX, passageY] = NoWall;
                            }
                        }
                    }
                }
            }
            List<Coord> GetLine(Coord from, Coord to)
            {
                var line = new List<Coord>();

                int x = from.TileX;
                int y = from.TileY;
                int dx = to.TileX - from.TileX;
                int dy = to.TileY - from.TileY;

                bool inverted = false;

                int step = Math.Sign(dx);
                int gradientStep = Math.Sign(dy);

                int longest = Mathf.Abs(dx);
                int shortest = Mathf.Abs(dy);

                if (longest < shortest)
                {
                    inverted = true;
                    longest = Mathf.Abs(dy);
                    shortest = Mathf.Abs(dx);
                    step = Math.Sign(dy);
                    gradientStep = Math.Sign(dx);
                }

                int gradientAccumulation = longest / 2;

                for (int i = 0; i < longest; i++)
                {
                    line.Add(new Coord(x, y));

                    if (inverted)
                    {
                        y += step;
                    }
                    else
                    {
                        x += step;
                    }

                    gradientAccumulation += shortest;

                    if (gradientAccumulation >= longest)
                    {
                        if (inverted)
                        {
                            x += gradientStep;
                        }
                        else
                        {
                            y += gradientStep;
                        }

                        gradientAccumulation -= longest;
                    }
                }

                return line;
            }
        }
    }
}