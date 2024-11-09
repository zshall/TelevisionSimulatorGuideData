using System.Xml;
using System.Xml.Linq;

namespace TelevisionSimulatorGuideData {
    public class ListingFileService : IHostedService {
        public static XDocument ListingsDocument;

        private readonly IConfiguration _config;
        private readonly ILogger<ListingFileService> _logger;
        private string _xmlFilePath = "path/to/your/file.xml";
        private FileSystemWatcher _fileWatcher;

        public ListingFileService(IConfiguration config, ILogger<ListingFileService> logger)
        {
            _config = config;
            _logger = logger;
        }

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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _fileWatcher.EnableRaisingEvents = false;
            _fileWatcher.Dispose();
            return Task.CompletedTask;
        }

        private void OnXmlFileChanged(object sender, FileSystemEventArgs e)
        {
            LoadDocument();
        }

        private void LoadDocument()
        {
            var doc = new XmlDocument();
            doc.Load(_xmlFilePath);
            ListingsDocument = doc.ToXDocument();
            _logger.LogInformation("Listings File updated.");
        }
    }
}
