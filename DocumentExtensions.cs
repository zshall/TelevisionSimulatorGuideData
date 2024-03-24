using System.Xml.Linq;
using System.Xml;

namespace TelevisionSimulatorGuideData;

public static class DocumentExtensions
{
    public static XmlDocument ToXmlDocument(this XDocument xDocument)
    {
        var xmlDocument = new XmlDocument();
        using (var xmlReader = xDocument.CreateReader())
        {
            xmlDocument.Load(xmlReader);
        }
        return xmlDocument;
    }

    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
        using (var nodeReader = new XmlNodeReader(xmlDocument))
        {
            nodeReader.MoveToContent();
            return XDocument.Load(nodeReader);
        }
    }
}