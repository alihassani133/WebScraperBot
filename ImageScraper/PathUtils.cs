using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScraper
{
    internal static class PathUtils
    {
        public static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Unknown";
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name.Trim();
        }
    }
}
