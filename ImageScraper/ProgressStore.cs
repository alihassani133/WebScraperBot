using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScraper
{
    internal sealed class ProgressStore
    {
        private readonly string _path;
        private readonly HashSet<string> _seen;

        public ProgressStore(string path)
        {
            _path = path;
            _seen= LoadInternal(path);
        }

        public bool Contains(string url) => _seen.Contains(url);

        public void MarkDone(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            if (_seen.Add(url))
            {
                File.AppendAllText(_path, url + Environment.NewLine);
            }
        }

        public int Count => _seen.Count;

        private static HashSet<string> LoadInternal(string path)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "url\n");
            }

            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in File.ReadAllLines(path).Skip(1))
            {
                var v = line.Trim();
                if (!string.IsNullOrEmpty(v)) set.Add(v);
            }
            return set;
        }
    }
}
