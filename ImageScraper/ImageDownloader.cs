using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageScraper
{
    internal class ImageDownloader
    {
        /// <summary>
        /// Downloads an image via the page’s browser context (fetch in page JS) so traffic goes through the same tunnel/proxy.
        /// Returns the saved file path.
        /// </summary>
        public static async Task<string> DownloadWithPlaywrightAsync(IPage page, string imageUrl, string baseDir, string partNumber)
        {
            try
            {
                var safePart = PathUtils.Sanitize(partNumber ?? "Unknown");
                var folder = Path.Combine(baseDir, safePart);
                Directory.CreateDirectory(folder);

                var fileName = TryGetFileName(imageUrl);
                var filePath = Path.Combine(folder, fileName);

                // Use Blob + FileReader to avoid huge argument/spread limits in String.fromCharCode(...)
                var base64 = await page.EvaluateAsync<string>(
                    @"async (url) => {
                    const res = await fetch(url, { cache: 'no-store' });
                    if (!res.ok) throw new Error(`HTTP ${res.status}`);
                    const blob = await res.blob();

                    // Read as data URL to get proper base64 without manual encoding
                    const toBase64 = (b) => new Promise((resolve, reject) => {
                        const fr = new FileReader();
                        fr.onload = () => {
                            const result = fr.result; // 'data:<mime>;base64,AAAA...'
                            const comma = result.indexOf(',');
                            resolve(result.slice(comma + 1));
                        };
                        fr.onerror = reject;
                        fr.readAsDataURL(b);
                    });

                    return await toBase64(blob);
                }",
                    imageUrl
                );

                var bytes = Convert.FromBase64String(base64);
                await File.WriteAllBytesAsync(filePath, bytes);

                return filePath;
            }
            catch (Exception ex)
            {

                var status = TryParseHttpStatus(ex.Message);

                // Skip-only (no retry, no stop)
                if (status is 404 or 410)
                {
                    Console.WriteLine($"⚠️ Image missing (HTTP {status}). Skipping.");
                    return string.Empty; // signals: HadImage = false
                }

                // Transient: let RetryHelper retry
                if (status is 429 || (status >= 500 && status <= 599) || IsNetworky(ex))
                {
                    Console.WriteLine($"⚠️ Transient error (HTTP {status}). Will retry.");
                    throw; // RetryHelper will catch & retry
                }

                // Fatal client errors (VPN down / blocked etc.) -> stop program
                // 401/403 and any other 4xx we haven't white-listed
                if (status is >= 400 and <= 499)
                {
                    Console.WriteLine($"❌ Access error (HTTP {status}). Stopping.");
                    throw; // not transient -> RetryHelper will NOT retry; program stops
                }

                // Unknown -> treat as transient to be safe
                Console.WriteLine($"⚠️ Unexpected error: {ex.Message}. Will retry.");
                throw;
            }
        }

        private static string TryGetFileName(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var name = Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrWhiteSpace(name)) return name;
            }
            catch { /* ignore */ }

            var ext = InferImageExtension(imageUrl) ?? ".jpg";
            return "image_1" + ext;
        }

        private static string? InferImageExtension(string url)
        {
            var lower = url.ToLowerInvariant();
            if (lower.Contains(".png")) return ".png";
            if (lower.Contains(".webp")) return ".webp";
            if (lower.Contains(".jpeg")) return ".jpeg";
            if (lower.Contains(".jpg")) return ".jpg";
            if (lower.Contains(".gif")) return ".gif";
            return null;
        }

        static int? TryParseHttpStatus(string? message)
        {
            if (string.IsNullOrEmpty(message)) return null;
            // Our JS throws Error("HTTP <code>")
            var idx = message.IndexOf("HTTP ", StringComparison.OrdinalIgnoreCase);
            if (idx < 0 || idx + 5 >= message.Length) return null;
            var tail = message.Substring(idx + 5).Trim();
            var digits = new string(tail.TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(digits, out var code) ? code : null;
        }

        static bool IsNetworky(Exception ex)
        {
            var m = ex.Message ?? "";
            return ex is TimeoutException
                || ex is TaskCanceledException
                || m.Contains("net::", StringComparison.OrdinalIgnoreCase)
                || m.Contains("Target closed", StringComparison.OrdinalIgnoreCase);
        }
    }
}
