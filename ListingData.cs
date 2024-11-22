using System.Text.Json.Serialization;

namespace TelevisionSimulatorGuideData;

/// <remarks>
/// This is going to be inside a dictionary where the key is the channel ID so we don't need a channel ID property
/// </remarks>
public class ListingData : SpanInfo
{
    /// <summary>
    /// Channel ID
    /// </summary>
    [JsonIgnore]
    public string ChannelId { get; set; }

    /// <summary>
    /// The start time of the program
    /// </summary>
    public DateTimeOffset Start { get; set; }
    
    /// <summary>
    /// For guides themes that operate without fluid columns, this is a list of all timeslots it's displayed on (0-indexed)
    /// </summary>
    public List<int> Timeslots { get; set; } = [];

    /// <summary>
    /// The title of the program
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Category of the program
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether the program is broadcast in stereo
    /// </summary>
    public bool IsStereo { get; set; }

    /// <summary>
    /// Whether the program is subtitled
    /// </summary>
    public bool IsSubtitled { get; set; }

    /// <summary>
    /// The rating of the program
    /// </summary>
    public string? Rating { get; set; }
}

public class SpanInfo {
    /// <summary>
    /// The duration of the program in minutes.
    /// </summary>
    public int Span { get; set; }

    /// <summary>
    /// Whether the program continues from the previous timeslot (in the past I mean)
    /// </summary>
    public bool IsContinuedLeft { get; set; }

    /// <summary>
    /// Whether the program continues off-screen to the next timeslot
    /// </summary>
    public bool IsContinuedRight { get; set; }
}

public class ChannelData {
    /// <summary>
    /// Abbreviation of the channel
    /// </summary>
    public string? Abbr { get; set; }

    /// <summary>
    /// Channel number
    /// </summary>
    public int? Number { get; set; }

    /// <summary>
    /// Listings of what's currently on, for the number of timeslots we're showing
    /// </summary>
    public List<ListingData> Listings { get; set; } = new List<ListingData>();
}

public class TvslSchema {
    /// <summary>
    /// Channels in the guide
    /// </summary>
    public Dictionary<string, ChannelData> Channels { get; set; }
}

/// <summary>
/// Constants
/// </summary>
public static class Metadata
{
    /// <summary>
    /// List of categories that are color-coded in the guide
    /// </summary>
    public static List<string> ColorCodedCategories = new List<string> { "sports event", "news", "kids", "movie" };
}