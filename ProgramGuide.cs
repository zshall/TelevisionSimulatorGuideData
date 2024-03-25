using System.Xml;
using System.Xml.XPath;

namespace TelevisionSimulatorGuideData;

public class ProgramGuide
{
    public Dictionary<string, IEnumerable<GuideData>> GetData(string? listingsFile, DateTimeOffset? start = null)
    {
        ArgumentNullException.ThrowIfNull(listingsFile);

        start ??= DateTimeOffset.Now;
        var doc = new XmlDocument();
        doc.Load(listingsFile);
        var xDoc = doc.ToXDocument();

        var startingTimeslot = (int)Math.Floor(GetTimeslot(start.Value));
        var numberOfTimeslots = 3;

        var timeslots = Enumerable.Range(startingTimeslot, numberOfTimeslots);

        var channels = xDoc.XPathSelectElements("//channel").ToDictionary(k => k.Attribute("id").Value, e => e.Element("display-name").Value);

        var categoriesWeCareAbout = new List<string> { "sports event", "news", "kids", "movie" };

        var todaysPrograms = xDoc.XPathSelectElements(@"//programme[
		date = '20240324'
	]").Select(p => new {
            Id = p.Attribute("channel").Value,
            Start = GetTimeslot(p.Attribute("start").Value),
            End = GetTimeslot(p.Attribute("stop").Value),
            Title = p.Element("title").Value,
            Category = p.Elements("category").Where(p => categoriesWeCareAbout.Contains(p.Value, StringComparer.OrdinalIgnoreCase)).FirstOrDefault()?.Value,
            Stereo = p.XPathSelectElement("//stereo")?.Value,
            Subtitles = p.XPathSelectElement("//subtitles")?.Attribute("type")?.Value,
            Rating = p.XPathSelectElement("//rating[@system='MPAA']/value")?.Value
        }).OrderBy(p => p.Start).GroupBy(p => p.Id);

        var currentTimeslotsPrograms = todaysPrograms.Select(group => {
            return timeslots.Select(i => group.Aggregate((x, y) => Math.Abs(x.Start - i) < Math.Abs(y.Start - i) ? x : y)).DistinctBy(p => p.Start);
        });

        return currentTimeslotsPrograms.ToDictionary(key => key.FirstOrDefault().Id, group => {
            return group.Select((p, i) => new GuideData {
                Channel = channels[p.Id],
                Timeslot = i,
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
    
    private static double GetTimeslot(string? dateString)
    {
        var converted = ConvertDateString(dateString);
        return GetTimeslot(converted);
    }

    private static double GetTimeslot(DateTimeOffset d)
    {
        return (d.Hour * 2) + Math.Round((double)d.Minute / 60, 3);
    }

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