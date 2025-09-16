using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ReadProductInfo
{

    public static class CsvManager
    {
        public static void EnsureCsvHeader(string path)
        {
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.AppendAllText(path, "Number,Part-Number,Description,Webpage Link,Price,Specifications,KitContent\n");
            }
        }

        public static void AppendProductToCsv(ProductInfo p, string path)
        {
            //string imgFolder = string.IsNullOrEmpty(p.ImagePath)
            // ? ""
            // : Path.GetDirectoryName(p.ImagePath)?.Replace("\\", "/");

            //string imgLink = string.IsNullOrEmpty(imgFolder)
            //    ? ""
            //    : $"=HYPERLINK(\"{imgFolder}\"";


            var line = $"{p.Number},\"{Escape(p.PartNumberHtml)}\",\"{Escape(p.DescriptionHtml)}\",\"{p.WebpageLink}\",\"{Escape(p.PriceHtml)}\",\"{Escape(p.SpecificationsHtml)}\",\"{Escape(p.KitContentHtml)}\"\n";

            File.AppendAllText(path, line, Encoding.UTF8);
        }

        public static string Escape(string input)
        {
            return input.Replace("\"", "\"\"").Trim();
        }

        public static HashSet<int> GetAlreadyScrapedRows(string path)
        {
            var set = new HashSet<int>();

            if (!File.Exists(path))
                return set;

            foreach (var line in File.ReadLines(path).Skip(1))
            {
                var firstColumn = line.Split(',').FirstOrDefault();
                if (int.TryParse(firstColumn, out int rowNum))
                {
                    set.Add(rowNum);
                }
            }

            return set;
        }

        public static List<string> ReadLinks(string path)
        {
            var links = new List<string>();
            foreach (var line in File.ReadLines(path).Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    string url = parts[1].Trim('"');
                    if (!url.StartsWith("[FAILED]"))
                        links.Add(url);
                }
            }
            return links;
        }
    }

}
