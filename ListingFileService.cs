using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TelevisionSimulatorGuideData {
    public class ListingFileService : IHostedService {
        public static XDocument? ListingsDocument;
        public static Dictionary<string, ChannelData> Channels;

        private readonly IConfiguration _config;
        private readonly ILogger<ListingFileService> _logger;
        private string _xmlFilePath = "path/to/your/file.xml";
        private FileSystemWatcher _fileWatcher;

        public ListingFileService(IConfiguration config, ILogger<ListingFileService> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Starts the file watcher and loads the listings file.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_config.GetSection("ListingsFile").Value)) {
                throw new ArgumentNullException("ListingsFile", "ListingsFile configuration is required.");
            }

            _xmlFilePath = _config.GetSection("ListingsFile").Value!;
            LoadDocument();
            _logger.LogInformation($"Watching file: {_xmlFilePath}");

            _fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(_xmlFilePath)) {
                Filter = Path.GetFileName(_xmlFilePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            _fileWatcher.Changed += OnXmlFileChanged;
            _fileWatcher.EnableRaisingEvents = true;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the service
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reloads the listing data when the XML file changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnXmlFileChanged(object sender, FileSystemEventArgs e)
        {
            LoadDocument();
        }

        /// <summary>
        /// Loads the listings file into an XDocument and pre-computes list of channels.
        /// </summary>
        private void LoadDocument()
        {
            var doc = new XmlDocument();
            doc.Load(_xmlFilePath);
            ListingsDocument = doc.ToXDocument();
            _logger.LogInformation("Listings File updated.");

            Channels = ListingsDocument.XPathSelectElements("//channel").ToDictionary(k => k.Attribute("id").Value, e =>
                    new {
                        Number = e.XPathSelectElements("display-name")
                            .FirstOrDefault(p => Regex.IsMatch(p.Value, @"^\d+$"))?.Value,
                        Abbr = e.XPathSelectElements("display-name")
                            .FirstOrDefault(p => Regex.IsMatch(p.Value, @"^[A-Z]+[A-Z0-9]*$"))?.Value
                    }).OrderBy(kv =>
                    string.IsNullOrWhiteSpace(kv.Value.Number) ? int.MaxValue : int.Parse(kv.Value.Number))
                .ToDictionary(
                    k => k.Key,
                    v => new ChannelData {
                        Abbr = v.Value.Abbr,
                        Number = string.IsNullOrWhiteSpace(v.Value.Number) ? null : int.Parse(v.Value.Number)
                    }
                );
        }

        /// <summary>
        /// Gets all programs for a specific time range using the `start` and `stop` attributes of the `programme` element.
        /// </summary>
        /// <param name="fromDateTime"></param>
        /// <param name="toDateTime"></param>
        /// <param name="channels"></param>
        public static IEnumerable<XElement> GetProgramsForTimeRange(DateTimeOffset fromDateTime,
            DateTimeOffset toDateTime, List<string>? channels = null)
        {
            if (null == ListingsDocument) {
                throw new InvalidOperationException("Listings document is not loaded.");
            }

            return ListingsDocument.Descendants("programme")
                .Where(p => {
                    var start = p.Attribute("start").Value.ToDateTimeOffsetFromXmlTvTime();
                    var stop = p.Attribute("stop").Value.ToDateTimeOffsetFromXmlTvTime();
                    return (null == channels || channels.Contains(p.Attribute("channel")?.Value))
                        && !(start < fromDateTime && stop < fromDateTime) && !(start > toDateTime && stop > toDateTime);
                });
        }
    }
}
