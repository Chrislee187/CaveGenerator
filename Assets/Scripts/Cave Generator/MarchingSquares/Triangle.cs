public readonly struct Triangle
{
    public readonly int VertexIndexA;
    public readonly int VertexIndexB;
    public readonly int VertexIndexC;

    private readonly int[] _vertices;
    public Triangle(int vertexIndexA, int vertexIndexB, int vertexIndexC)
    {
        VertexIndexA = vertexIndexA;
        VertexIndexB = vertexIndexB;
        VertexIndexC = vertexIndexC;

        _vertices = new[] {vertexIndexA, vertexIndexB, vertexIndexC};
    }

    public int this[int i] => _vertices[i];

    public bool Contains(int vertexIndex) => vertexIndex == VertexIndexA 
                                             || vertexIndex == VertexIndexB 
                                             || vertexIndex == VertexIndexC;
}