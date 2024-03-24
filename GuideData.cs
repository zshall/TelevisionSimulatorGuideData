namespace TelevisionSimulatorGuideData;

public class GuideData
{
    public string? Channel { get; set; }
    public int Timeslot { get; set; }
    public bool IsContinuedLeft { get; set; }
    public bool IsContinuedRight { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public bool IsStereo { get; set; }
    public bool IsSubtitled { get; set; }
    public string? Rating { get; set; }
}