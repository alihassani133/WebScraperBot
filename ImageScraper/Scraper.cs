using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScraper
{
    internal static class Scraper
    {
        private const string PartPrimary = ".custom-partNumberGroup h1 .partNo";
        private const string PartFallBack = "ul.breadcrumb.breadcrumb-bg.noPrint li.active";

        private const string ImagePrimary = "#currentImg img.theImg";
        private const string ImageFallback = "img.theImg";

        public static async Task<string?> GetPartNumberAsync(IPage page)
        {

            // Primary
            var primary = await page.QuerySelectorAsync(PartPrimary);
            if (primary is not null)
            {
                var txt = (await primary.InnerTextAsync())?.Trim();
                if (!string.IsNullOrWhiteSpace(txt)) return txt;
            }

            // Fallback (breadcrumb active) 
            var fallback = await page.QuerySelectorAsync(PartFallBack);
            if (fallback is not null)
            {
                var txt = (await fallback.InnerTextAsync())?.Trim();
                if (string.IsNullOrWhiteSpace(txt)) return txt;
            }

            return null;
        }

        /// <summary>
        /// Returns a single image URL (absolute) or null if none found.
        /// </summary>
        public static async Task<string?> GetOneImageUrlAsync(IPage page)
        {
            // Prefer the main image 
            var img = await page.QuerySelectorAsync(ImagePrimary)
                ?? (await page.QuerySelectorAllAsync(ImageFallback)).FirstOrDefault();

            if (img is null) return null;

            // Try src, then data-src, then srcset
            var src = await img.GetAttributeAsync("src");
            if (IsValidUrlish(src))
                return MakeAbsolute(page.Url, src);


            var dataSrc = await img.GetAttributeAsync("data-src");
            if (IsValidUrlish(dataSrc))
                return MakeAbsolute(page.Url, dataSrc!);

            var srcset = await img.GetAttributeAsync("srcset");
            if (!string.IsNullOrWhiteSpace(srcset))
            {
                var best = PickBestFromSrcset(srcset!);
                if (IsValidUrlish(best))
                {
                    return MakeAbsolute(page.Url, best!);
                }
            }

            return null;
        }

        public static bool IsValidUrlish(string? s) => !string.IsNullOrWhiteSpace(s);

        private static string? MakeAbsolute(string baseUrl, string maybaRelative)
        {
            try
            {
                var baseUri = new Uri(baseUrl);
                var uri = new Uri(baseUri, maybaRelative);
                return uri.ToString();
            }
            catch { return null; }
        }

        /// <summary>
        /// Very simple "best" choice: pick the last candidate (often the largest).
        /// </summary>
        private static string? PickBestFromSrcset(string srcset)
        {
            // srcset example: "a.jpg 320w, b.jpg 640w, c.jpg 1280w"
            var parts = srcset.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0) return null;

            var last = parts.Last();
            var spaceIdx = last.IndexOf(' ');

            return spaceIdx >= 0 ? last[..spaceIdx].Trim() : last.Trim();
        }
    }
}
