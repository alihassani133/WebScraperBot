using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScraper
{
    internal static class CsvLinksReader
    {
        /// <summary>
        /// Reads product page URLs from the 4th column of a CSV (skips header).
        /// </summary>
        public static List<string> ReadFourthColumnUrls(string csvPath)
        {
            var urls = new List<string>();
            using var reader = new StreamReader(csvPath);
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                DetectDelimiter = true,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            };

            using var csv = new CsvReader(reader, configuration);

            // header
            csv.Read();
            csv.ReadHeader();

            while (csv.Read())
            {
                try
                {
                    var url = csv.GetField(3);
                    if (!string.IsNullOrWhiteSpace(url))
                        urls.Add(url.Trim());
                }
                catch
                {

                    // Ignore malformed rows
                }
            }

            return urls;
        }

        /// <summary>
        /// Returns every CSV under links/[category]/*.csv
        /// </summary>
        public static List<string> DiscoverLinkCsvs(string linksroot)
        {
            if (!Directory.Exists(linksroot)) return new List<string>();
            var list = Directory.GetDirectories(linksroot)
                .SelectMany(cat => Directory.GetFiles(cat, "*.csv", SearchOption.TopDirectoryOnly))
                .ToList();
            return list;
        }
    }
}
