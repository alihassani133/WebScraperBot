using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadProductInfo
{
    class ScrapedLinksTracker
    {
        private static readonly string filePath = "ScrapedLinks.csv";
        private static readonly HashSet<string> _scrapedLinks = LoadLinks();
        private static HashSet<string> LoadLinks()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(filePath))
            {
                foreach (var line in File.ReadLines(filePath))
                {
                    var trimmed = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        set.Add(trimmed);
                }
            }
            return set;
        }
        public static bool IsTheLinkScraped(string link)
        {
            return _scrapedLinks.Contains(link.Trim());
        }
        public static void WriteInTheScrapedLinksList(string link)
        {
            string trimmed = link.Trim();
            if (!_scrapedLinks.Contains(trimmed))
            {
                File.AppendAllText(filePath, trimmed + Environment.NewLine, Encoding.UTF8);
                _scrapedLinks.Add(trimmed);
            }
        }
    }
}
