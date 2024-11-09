using System.Collections.Immutable;

namespace TelevisionSimulatorGuideData;

/// <remarks>
/// This is going to be inside of a dictionary where the key is the channel ID so we don't need a channel ID property
/// </remarks>
public class ListingData
{
    /// <summary>
    /// The start time in minutes from midnight.
    /// (Based on how the data is used I'm not sure if this will be useful since we start the guide from now)
    /// </summary>
    public int Start { get; set; }

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

public class ChannelData {
    /// <summary>
    /// Abbreviation of the channel
    /// </summary>
    public string? Abbr { get; set; }

    /// <summary>
    /// Channel number
    /// </summary>
    public int? Number { get; set; }
}

public class TvslSchema {
    /// <summary>
    /// Listings of what's currently on, for the number of timeslots we're showing
    /// </summary>
    public Dictionary<string, IEnumerable<ListingData>> Listings { get; set; }

    /// <summary>
    /// Channels in the guide
    /// </summary>
    public ImmutableSortedDictionary<string, ChannelData> Channels { get; set; }
}