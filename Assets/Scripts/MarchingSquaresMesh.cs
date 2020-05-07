using System.Collections.Generic;
using UnityEngine;

public class MarchingSquaresMesh
{

    public Vector3[] Vertices => _vertices.ToArray();
    public int[] Triangles => _triangles.ToArray();

    private SquareGrid _squareGrid;

    private readonly List<Vector3> _vertices = new List<Vector3>();
    private readonly List<int> _triangles = new List<int>();

    public MarchingSquaresMesh(int[,] map, float squareSize)
    {
        _squareGrid = new SquareGrid(map, squareSize);

        _vertices.Clear();
        _triangles.Clear();

        For.Xy(_squareGrid.Width, _squareGrid.Height, (x, y) =>
        {
            TriangulateSquare(_squareGrid[x, y]);
        });
    }

    private void TriangulateSquare(Square square)
    {
        // Create mesh from the specified configuration.
        // See <see href="https://en.wikipedia.org/wiki/Marching_squares#Basic_algorithm">"Look-up table contour lines"</see>
        switch (square.Configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.CenterBottom, square.BottomLeft, square.CenterLeft);
                break;
            case 2:
                MeshFromPoints(square.CenterRight, square.BottomRight, square.CenterBottom);
                break;
            case 4:
                MeshFromPoints(square.CenterTop, square.TopRight, square.CenterRight);
                break;
            case 8:
                MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.CenterRight, square.BottomRight, square.BottomLeft, square.CenterLeft);
                break;
            case 6:
                MeshFromPoints(square.CenterTop, square.TopRight, square.BottomRight, square.CenterBottom);
                break;
            case 9:
                MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterBottom, square.BottomLeft);
                break;
            case 12:
                MeshFromPoints(square.TopLeft, square.TopRight, square.CenterRight, square.CenterLeft);
                break;
            case 5:
                MeshFromPoints(square.CenterTop, square.TopRight, square.CenterRight, square.CenterBottom, square.BottomLeft, square.CenterLeft);
                break;
            case 10:
                MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterRight, square.BottomRight, square.CenterBottom, square.CenterLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.CenterTop, square.TopRight, square.BottomRight, square.BottomLeft, square.CenterLeft);
                break;
            case 11:
                MeshFromPoints(square.TopLeft, square.CenterTop, square.CenterRight, square.BottomRight, square.BottomLeft);
                break;
            case 13:
                MeshFromPoints(square.TopLeft, square.TopRight, square.CenterRight, square.CenterBottom, square.BottomLeft);
                break;
            case 14:
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.CenterBottom, square.CenterLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(square.TopLeft, square.TopRight, square.BottomRight, square.BottomLeft);
                break;
        }
    }

    private void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    private void AssignVertices(Node[] points)
    {
        foreach (var t in points)
        {
            if (t.VertexIndex == -1)
            {
                t.VertexIndex = _vertices.Count;
                _vertices.Add(t.Position);
            }
        }
    }

    private void CreateTriangle(Node a, Node b, Node c)
    {
        _triangles.Add(a.VertexIndex);
        _triangles.Add(b.VertexIndex);
        _triangles.Add(c.VertexIndex);
    }

    private class SquareGrid
    {
        private readonly Square[,] _squares;

        public readonly int Width;
        public readonly int Height;

        public SquareGrid(int[,] map, float squareSize)
        {
            var nodeCountX = map.GetLength(0);
            var nodeCountY = map.GetLength(1);
            Width = nodeCountX - 1;
            Height = nodeCountY - 1;
            var mapWidth = nodeCountX * squareSize;
            var mapHeight = nodeCountY * squareSize;

            var controlNodes = new ControlNode[nodeCountX, nodeCountY];

            For.Xy(nodeCountX, nodeCountY, (x, y) =>
            {
                var position = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                controlNodes[x, y] = new ControlNode(position, map[x, y] == 1, squareSize);
            });

            _squares = new Square[Width, Height];

            For.Xy(Width, Height, (x, y) =>
            {
                _squares[x, y] = new Square(
                    controlNodes[x, y + 1],
                    controlNodes[x + 1, y + 1],
                    controlNodes[x + 1, y],
                    controlNodes[x, y]
                );
            });

        }

        public Square this[int x, int y] => _squares[x, y];
    }

    private class Square
    {
        /*  https://en.wikipedia.org/wiki/Marching_squares
         *  ControlNodes represent the original "wall" squares from the map
         *  Nodes represent the intermediate points we use to generate the wall mesh
         *  depending on which ControlNodes (walls) are in place.
         *
         *          TopLeft     centerTop       TopRight
         *             +------------+-------------+
         *             |                          | 
         *             |                          | 
         * centerLeft  |                          | centerRight
         *             |                          | 
         *             |                          | 
         *             +------------+-------------+
         *          botLeft     centerBot       BotRight
         */
        public readonly ControlNode TopLeft;
        public readonly ControlNode TopRight;
        public readonly ControlNode BottomRight;
        public readonly ControlNode BottomLeft;

        public readonly Node CenterTop;
        public readonly Node CenterRight;
        public readonly Node CenterBottom;
        public readonly Node CenterLeft;

        public int Configuration;

        private class NodeConfigurationValues
        {
            public const int BottomLeft = 1;
            public const int BottomRight = 2;
            public const int TopRight = 4;
            public const int TopLeft = 8;
        }
        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;

            CenterTop = TopLeft.Right;
            CenterRight = BottomRight.Above;
            CenterBottom = BottomLeft.Right;
            CenterLeft = BottomLeft.Above;

            CalculateConfiguration();
        }

        private void CalculateConfiguration()
        {
            if (TopLeft.Active)
            {
                Configuration += NodeConfigurationValues.TopLeft;
            }

            if (TopRight.Active)
            {
                Configuration += NodeConfigurationValues.TopRight;
            }

            if (BottomRight.Active)
            {
                Configuration += NodeConfigurationValues.BottomRight;
            }

            if (BottomLeft.Active)
            {
                Configuration += NodeConfigurationValues.BottomLeft;
            }
        }
    }

    private class Node
    {
        public readonly Vector3 Position;
        public int VertexIndex = -1;

        // ReSharper disable once MemberCanBeProtected.Global - R# lies, it can't just be protected as the ControlNode can't access the constructor
        public Node(Vector3 position)
        {
            Position = position;
        }
    }

    private class ControlNode : Node
    {
        public readonly bool Active;
        public readonly Node Above;
        public readonly Node Right;

        public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
        {
            Active = active;
            Above = new Node(position + Vector3.forward * squareSize / 2);
            Right = new Node(position + Vector3.right * squareSize / 2);
        }
    }
}