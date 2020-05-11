public class CaveSettings
{
    public CaveSettings(string seed, int randomFillPercent, int borderSize = 1, int smoothingIterations = 5, bool processRegions = true, int smallWallThresholdSize = 50, int smallRoomThresholdSize = 50, bool ensureAllRoomsConnected = true, int interconnectingPassageWidth = 1)
    {
        Seed = seed;
        RandomFillPercent = randomFillPercent;
        BorderSize = borderSize;
        SmoothingIterations = smoothingIterations;
        ProcessRegions = processRegions;
        SmallWallThresholdSize = smallWallThresholdSize;
        SmallRoomThresholdSize = smallRoomThresholdSize;
        EnsureAllRoomsConnected = ensureAllRoomsConnected;
        InterconnectingPassageWidth = interconnectingPassageWidth;
    }

    public string Seed { get; set; }
    public int RandomFillPercent { get; set; }
    public int BorderSize { get; set; }
    public int SmoothingIterations { get; set; }
    public bool ProcessRegions { get; set; }
    public int SmallWallThresholdSize { get; set; }
    public int SmallRoomThresholdSize { get; set; }
    public bool EnsureAllRoomsConnected { get; set; }
    public int InterconnectingPassageWidth { get; set; }
}