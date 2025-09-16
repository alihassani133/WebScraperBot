using Microsoft.Playwright;
using System;

namespace ImageScraper
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new AppConfig();

            
            Directory.CreateDirectory(config.OutputRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(config.FullLogCsv)!);

            if (!File.Exists(config.ProgressCsv))
                File.WriteAllText(config.ProgressCsv, "url\n"); 

            if (!File.Exists(config.FullLogCsv))
                File.WriteAllText(config.FullLogCsv, "No,PartNumber,PageUrl,ImageUrl,HadImage\n"); 

            var progress = new ProgressStore(config.ProgressCsv);
            var logger = new FullLogger(config.FullLogCsv);


            Console.WriteLine($"Loaded {progress.Count} previously-scraped links.");


            var csvFiles = CsvLinksReader.DiscoverLinkCsvs(config.LinksRoot);

            if (csvFiles.Count == 0)
            {
                Console.WriteLine("No CSV files found under ./links/**");
                return;
            }

            
            var retry = new RetryHelper(config.MaxRetriesPerDownload, config.RetryDelay);

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                foreach (var csv in csvFiles)
                {
                    var urls = CsvLinksReader.ReadFourthColumnUrls(csv);

                    Console.WriteLine($"\nProcessing CSV: {csv}");
                    Console.WriteLine($"  Found {urls.Count} URLs (4th column, header skipped).");

                    foreach (var url in urls)
                    {
                        if (string.IsNullOrWhiteSpace(url)) continue;

                        if (progress.Contains(url))
                        {
                            Console.WriteLine($"  SKIP (already): {url}");
                            continue;
                        }

                        Console.WriteLine($"  Visiting: {url}");
                        await page.GotoAsync(url, new PageGotoOptions
                        {
                            Timeout = config.NavigationTimeOutMs,
                            WaitUntil = WaitUntilState.NetworkIdle
                        });

                        var partNumber = await Scraper.GetPartNumberAsync(page);
                        var sanitizedPart = PathUtils.Sanitize(partNumber ?? "Unknown");

                        var imageUrl = await Scraper.GetOneImageUrlAsync(page);
                        bool hadImage = !string.IsNullOrWhiteSpace(imageUrl);

                        if (hadImage)
                        {
                            var destFolder = Path.Combine(config.OutputRoot, sanitizedPart);
                            Directory.CreateDirectory(destFolder);

                            // Download with retry via Playwright; stop program if all attempts fail
                            string savedPath = string.Empty;
                            await retry.ExecuteAsync(async () =>
                            {
                                savedPath = await ImageDownloader.DownloadWithPlaywrightAsync(page, imageUrl!, config.OutputRoot, sanitizedPart);
                                //if (string.IsNullOrEmpty(savedPath))
                                //    throw new Exception("Playwright-based image download returned empty path.");
                            });
                            if (!string.IsNullOrEmpty(savedPath))
                            {
                                Console.WriteLine($"   Saved image for [{sanitizedPart}] -> {Path.GetFileName(savedPath)}");
                                hadImage = true;
                            }
                            else
                            {
                                Console.WriteLine($"   No downloadable image for [{sanitizedPart}]");
                                hadImage = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine($" No image found for [{sanitizedPart}]");
                        }

                        logger.Log(partNumber, url, imageUrl, hadImage);
                        progress.MarkDone(url);
                    }
                }
                Console.WriteLine("\nAll done.");
            }
            catch (RetryHelper.GiveUpException ex)
            {
                Console.Error.WriteLine($"\nStopping after repeated download failures: {ex.InnerException?.Message ?? ex.Message}");
                Environment.ExitCode = 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"\n❌ Unhandled error: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }
    }
}
