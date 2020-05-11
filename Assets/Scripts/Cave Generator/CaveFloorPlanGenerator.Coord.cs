public partial class CaveMapGenerator
{
    public readonly struct Coord
    {
        public readonly int TileX;
        public readonly int TileY;

        public Coord(int tileX, int tileY)
        {
            TileX = tileX;
            TileY = tileY;
        }
    }
}