using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Playwright;

namespace ReadProductInfo
{
    public static class ImageDownloader
    {
        public static async Task<string> DownloadImageWithPlaywrightAsync(IPage page, string imageUrl, string baseDir, string partNumber)
        {
            try
            {
                string folder = Path.Combine(baseDir, partNumber);
                Directory.CreateDirectory(folder);

                string fileName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
                string filePath = Path.Combine(folder, fileName);

                var base64 = await page.EvaluateAsync<string>(@"
                async (url) => {
                    const response = await fetch(url);
                    const buffer = await response.arrayBuffer();
                    return btoa(String.fromCharCode(...new Uint8Array(buffer)));
                }
            ", imageUrl);

                var bytes = Convert.FromBase64String(base64);
                await File.WriteAllBytesAsync(filePath, bytes);

                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Playwright image download failed: {ex.Message}");
                return "";
            }
        }
    }


}
