using System.Xml.Linq;
using System.Xml;

namespace TelevisionSimulatorGuideData;

public static class Extensions
{
    /// <summary>
    /// Converts an XmlDocument to an XDocument.
    /// </summary>
    /// <param name="xmlDocument"></param>
    /// <returns></returns>
    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
        using (var nodeReader = new XmlNodeReader(xmlDocument))
        {
            nodeReader.MoveToContent();
            return XDocument.Load(nodeReader);
        }
    }

    /// <summary>
    /// Converts a date string to a DateTimeOffset.
    /// </summary>
    /// <param name="dateString">Date in "yyyyMMddHHmmss zzz" format</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DateTimeOffset ToDateTimeOffsetFromXmlTvTime(this string? dateString) {
        string format = "yyyyMMddHHmmss zzz";
        DateTimeOffset result;

        if (DateTimeOffset.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out result)) {
            return result;
        }

        throw new ArgumentException("Invalid date string format.");
    }
}