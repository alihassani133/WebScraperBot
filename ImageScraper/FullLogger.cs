using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScraper
{
    internal sealed class FullLogger
    {
        private readonly string _path;
        private long _nextNo;

        public FullLogger(string path)
        {
            _path = path;
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "No,PartNumber,PageUrl,ImageUrl,HadImage\n");
                _nextNo = 1;
            }
            else
            {
                // Count existing data rows (minus header)
                var lines = File.ReadAllLines(_path).Length;
                _nextNo = Math.Max(1, lines - 1 + 1);
            }
        }

        public void Log(string? partNumber, string pageUrl, string? imageUrl, bool hadImage)
        {

            partNumber ??= String.Empty;
            imageUrl ??= string.Empty;

            var line = string.Join(",",
                _nextNo,
                Escape(partNumber),
                Escape(pageUrl),
                Escape(imageUrl),
                hadImage ? "true" : "false");

            File.AppendAllText(_path, line + Environment.NewLine);
            _nextNo++;
        }

        // Minimal Csv escaping for commas/quotes/newlines
        private static string Escape(string input)
        {
            if (input.Contains('"') || input.Contains(',') || input.Contains('\n') || input.Contains('\r'))
            {
                return "\"" + input.Replace("\"", "\"\"") + "\"";
            }
            return input;
        }
    }
}
