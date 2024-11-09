using System.Collections.Immutable;

namespace TelevisionSimulatorGuideData;

/// <remarks>
/// This is going to be inside of a dictionary where the key is the channel ID so we don't need a channel ID property
/// </remarks>
public class ListingData
{
    public int Timeslot { get; set; }
    public int Span { get; set; }
    public bool IsContinuedLeft { get; set; }
    public bool IsContinuedRight { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public bool IsStereo { get; set; }
    public bool IsSubtitled { get; set; }
    public string? Rating { get; set; }
}

public class ChannelData {
    public string? Abbr { get; set; }
    public int? Number { get; set; }
}

public class TvslSchema {
    public Dictionary<string, IEnumerable<ListingData>> Listings { get; set; }
    public ImmutableSortedDictionary<string, ChannelData> Channels { get; set; }
}