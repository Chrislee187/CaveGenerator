using System.Collections.Generic;

public partial class CaveMapGenerator
{
    public partial class Regions
    {
        public class Region : List<Coord>
        {
            public bool IsRoom { get; }
            public Region(bool isRoom)
            {
                IsRoom = isRoom;
            }

        }
    }
}