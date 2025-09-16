using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScraper
{
    public sealed class AppConfig
    {
        public string LinksRoot { get; init; }
        public string OutputRoot { get; init; }
        public string ProgressCsv { get; init; }
        public string FullLogCsv { get; init; }

        public int MaxRetriesPerDownload { get; init; } = 4;
        public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(5);
        public int NavigationTimeOutMs { get; init; } = 90_000;
        public int RequestTimeOutMs { get; init; } = 90_000;
        public AppConfig()
        {
            var baseDir = AppContext.BaseDirectory;
            LinksRoot = Path.Combine(baseDir, "links");
            OutputRoot = Path.Combine(baseDir, "ProductImages");
            ProgressCsv = Path.Combine(baseDir, "AlreadyScrapedLinks.csv");
            FullLogCsv = Path.Combine(baseDir, "logs", "FullLog.csv");
        }
    }
}
