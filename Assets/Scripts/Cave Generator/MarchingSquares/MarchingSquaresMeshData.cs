using System.Collections.Generic;
using UnityEngine;

public class MarchingSquaresMeshData
{

    public Vector3[] Vertices => _vertices.ToArray();
    public int[] Triangles => _triangles.ToArray();

    // TODO: Shouldn't really expose the inner list here
    public readonly Dictionary<int, List<Triangle>> TriangleDictionary = new Dictionary<int, List<Triangle>>();

    public readonly HashSet<int> CheckedVertices = new HashSet<int>();

    private readonly List<Vector3> _vertices = new List<Vector3>();
    private readonly List<int> _triangles = new List<int>();

    public MarchingSquaresMeshData(int[,] map, float squareSize)
    {
        var squareGrid = new Grid(map, squareSize);

        For.Xy(squareGrid.Width, squareGrid.Height, (x, y) =>
        {
            TriangulateSquare(squareGrid[x, y]);
        });
    }

    private void TriangulateSquare(Square square)
    {
        // Create mesh from the specified contour configuration.
        // See <see href="https://en.wikipedia.org/wiki/Marching_squares#Basic_algorithm">"Look-up table contour lines"</see>
        switch (square.ContourConfiguration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.CenterLeft, square.CenterBottom, square.BottomLeft);
                break;
            case 2:
                MeshFromPoints(square.BottomRight, square.CenterBottom, square.CenterRight);
                break;
            case 4:
                MeshFromPoints(square.TopRight, square.CenterRight, square.CenterTop);
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
                CheckedVertices.Add(square.TopLeft.VertexIndex);
                CheckedVertices.Add(square.TopRight.VertexIndex);
                CheckedVertices.Add(square.BottomRight.VertexIndex);
                CheckedVertices.Add(square.BottomLeft.VertexIndex);
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

        var triangle = new Triangle(a.VertexIndex, b.VertexIndex, c.VertexIndex);
        AddTriangleToDictionary(triangle.VertexIndexA, triangle);
        AddTriangleToDictionary(triangle.VertexIndexB, triangle);
        AddTriangleToDictionary(triangle.VertexIndexC, triangle);
    }

    private void AddTriangleToDictionary(int vertexIndex, Triangle triangle)
    {
        if (TriangleDictionary.ContainsKey(vertexIndex))
        {
            TriangleDictionary[vertexIndex].Add(triangle);
        }
        else
        {
            TriangleDictionary.Add(vertexIndex, new List<Triangle> {triangle});
        }
        
    }

    private class Grid
    {
        private readonly Square[,] _squares;

        public readonly int Width;
        public readonly int Height;

        public Grid(int[,] map, float squareSize)
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

        public int ContourConfiguration { get; private set; }

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

            CalculateContourConfiguration();
        }

        private void CalculateContourConfiguration()
        {
            const int bottomLeftBit = 1;
            const int bottomRightBit = 2;
            const int topRightBit = 4;
            const int topLeftBit = 8;

            if (TopLeft.Active)
            {
                ContourConfiguration += topLeftBit;
            }

            if (TopRight.Active)
            {
                ContourConfiguration += topRightBit;
            }

            if (BottomRight.Active)
            {
                ContourConfiguration += bottomRightBit;
            }

            if (BottomLeft.Active)
            {
                ContourConfiguration += bottomLeftBit;
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