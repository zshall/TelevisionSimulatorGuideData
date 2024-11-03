using System.Collections.Immutable;
using System.Xml;
using System.Xml.XPath;

namespace TelevisionSimulatorGuideData;

public class ProgramGuide
{
    /// <summary>
    /// Gets the current guide data in TVSL JSON format.
    /// </summary>
    /// <param name="listingsFile"></param>
    /// <param name="start"></param>
    /// <returns></returns>
    public ImmutableSortedDictionary<string, IEnumerable<GuideData>> GetData(string? listingsFile, DateTimeOffset? start = null)
    {
        ArgumentNullException.ThrowIfNull(listingsFile);

        start ??= DateTimeOffset.Now;
        var doc = new XmlDocument();
        doc.Load(listingsFile);
        var xDoc = doc.ToXDocument();

        var startingTimeslot = (int)Math.Floor(GetTimeslot(start.Value));
        var isThreeGrid = true; // whether we're showing 3 timeslots per row

        var numberOfTimeslots = isThreeGrid ? 3 : 1;
        var timeslots = Enumerable.Range(startingTimeslot, numberOfTimeslots);

        var channels = xDoc.XPathSelectElements("//channel").ToDictionary(k => k.Attribute("id").Value, e => e.Element("display-name").Value);

        var categoriesWeCareAbout = new List<string> { "sports event", "news", "kids", "movie" };

        var dateString = start.Value.ToString("yyyyMMdd");

        var todaysPrograms = xDoc.XPathSelectElements(@$"//programme[
		date = '{dateString}'
	]").Select(p => {
            var category = p.Elements("category").Where(p => categoriesWeCareAbout.Contains(p.Value, StringComparer.OrdinalIgnoreCase)).FirstOrDefault()?.Value.ToLowerInvariant();

            return new ListingRecord(
                p.Attribute("channel").Value,
                GetTimeslot(p.Attribute("start").Value),
                GetTimeslot(p.Attribute("stop").Value),
                p.Element("title").Value,
                category,
                p.XPathSelectElement("stereo")?.Value,
                p.XPathSelectElement("subtitles")?.Attribute("type")?.Value,
                p.XPathSelectElement($"rating[@system='{(category == "movie" ? "MPAA" : "VCHIP")}']/value")?.Value
            );
        }).OrderBy(p => p.Start).GroupBy(p => p.Id);

        var currentTimeslotsPrograms = todaysPrograms.Select(group => {
            return timeslots.Select(i => group.Aggregate((x, y) => Math.Abs(x.Start - i) < Math.Abs(y.Start - i) ? x : y)).DistinctBy(p => p.Start);
        });

        return currentTimeslotsPrograms.ToImmutableSortedDictionary(key => channels[key.FirstOrDefault().Id], group => {
            return group.Select((p, i) => new GuideData {
                Channel = channels[p.Id],
                Timeslot = i,
                Span = GetSpan(group, i, startingTimeslot, isThreeGrid),
                IsContinuedLeft = p.Start < startingTimeslot,
                IsContinuedRight = p.End > startingTimeslot + numberOfTimeslots,
                Title = p.Title,
                Category = p.Category,
                IsStereo = null != p.Stereo,
                IsSubtitled = null != p.Subtitles,
                Rating = p.Rating
            });
        });
    }

    /// <summary>
    /// Gets the span of the program in timeslots, up to the maximum number of timeslots.
    /// </summary>
    /// <param name="group">List of programs</param>
    /// <param name="i"></param>
    /// <param name="startingTimeslot"></param>
    /// <param name="numberOfTimeslots"></param>
    /// <returns></returns>
    private int GetSpan(IEnumerable<ListingRecord> group, int i, int startingTimeslot, bool isThreeGrid) {
        if (!isThreeGrid || group.Count() == 3) {
            return 1;
        }

        if (group.Count() == 1) {
            return 3;
        }

        // one of the programs needs to be 1 timeslot wide and the other needs to be 2.
        // not my finest code... it produces results but they're not always correct.
        // the problem with this method and why I've had so much trouble with it is that it really
        // requires us to know what the other programs have spanned; we're coalescing the closest programs
        // together and rounding off to the nearest timeslot rather than just showing the actual data.
        var first = group.First();

        if (i == 0) {
            return first.End > startingTimeslot + 1 ? 2 : 1;
        } else if (GetSpan(group, 0, startingTimeslot, isThreeGrid) == 2) {
            return 1;
        }

        return 2;
    }

    /// <summary>
    /// Gets the timeslot from a date string.
    /// </summary>
    /// <param name="dateString"></param>
    /// <returns></returns>
    private static double GetTimeslot(string? dateString)
    {
        var converted = ConvertDateString(dateString);
        return GetTimeslot(converted);
    }

    /// <summary>
    /// Gets the timeslot for the given date.
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    private static double GetTimeslot(DateTimeOffset d)
    {
        return (d.Hour * 2) + Math.Round((double)d.Minute / 60, 3);
    }

    /// <summary>
    /// Converts a date string to a DateTimeOffset.
    /// </summary>
    /// <param name="dateString"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static DateTimeOffset ConvertDateString(string? dateString)
    {
        string format = "yyyyMMddHHmmss zzz";
        DateTimeOffset result;

        if (DateTimeOffset.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out result))
        {
            return result;
        }
        else
        {
            throw new ArgumentException("Invalid date string format.");
        }
    }
}

internal record ListingRecord(string Id, double Start, double End, string Title, string? Category, string? Stereo, string? Subtitles, string? Rating);
