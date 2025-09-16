using CsvHelper;
using CsvHelper.Configuration;
using ExtractLinks;
using Microsoft.Playwright;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

class Program
{
    static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(90000);
        List<string> linksToExport = [];

        string toExportLinksCsvPath = "Categories.csv";

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false
        };
        using (StreamReader streamReader = new(toExportLinksCsvPath))
        using (CsvReader csvReader = new(streamReader, config))
        {
            while (csvReader.Read())
            {
                var link = csvReader.GetField(0);
                linksToExport.Add(link);
                Console.WriteLine(link);
            }
        }
        foreach (var linkToExport in linksToExport)
        {
            if (ScrapedCategoryLinksTracer.IsTheLinkScraped(linkToExport))
            {
                Console.WriteLine($"Skipping (already scraped): {linkToExport}");
                continue;
            }
            string webUrl = linkToExport;

            await page.GotoAsync(webUrl);

            var productLinks = new List<(int PageNumber, string Url)>();
            int currentPage = 1;

            var h1Text = await page.Locator("h1.media-heading.pageTitle.reg-heading").InnerTextAsync();

            while (true)
            {
                Console.WriteLine($"Processing page {currentPage}");

                IReadOnlyList<IElementHandle>? links;
                await page.WaitForSelectorAsync("table#buy-prods-table, table#browseTable");

                if (await page.QuerySelectorAsync("table#buy-prods-table") is IElementHandle)
                {
                      links = await page.QuerySelectorAllAsync("div.pd-see-all a");
                }
                else
                {

                    // Expand hidden rows to load details
                    var rows = await page.QuerySelectorAllAsync("tr[data-toggle='collapse']");
                    foreach (var row in rows)
                    {
                        await row.ClickAsync();
                        await page.WaitForTimeoutAsync(500); // wait for animations / DOM updates
                    }

                    // Wait for links to be available
                    await page.WaitForSelectorAsync("a.seeMoreDetails");

                    links = await page.QuerySelectorAllAsync("a.seeMoreDetails");
                }

                foreach (var link in links)
                {
                    string? url = null;
                    string text = await link.InnerTextAsync();
                    for (int attempt = 1; attempt <= 3; attempt++)
                    {
                        try
                        {
                            url = await link.GetAttributeAsync("href");
                            if (!string.IsNullOrEmpty(url))
                                break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Attempt {attempt} failed to get link: {ex.Message}");
                        }

                        await Task.Delay(3000); // wait 3 seconds before retry
                    }

                    if (string.IsNullOrEmpty(url))
                    {
                        url = "[FAILED]";
                    }

                    productLinks.Add((currentPage, url));
                }

                // Check if next page exists
                var nextButton = await page.QuerySelectorAsync("li.page-forward:not(.disabled) > a");

                if (nextButton == null)
                {
                    Console.WriteLine("No more pages.");
                    break;
                }

                await page.ClickAsync("li.page-forward:not(.disabled) > a");
                await page.WaitForTimeoutAsync(2000);
                currentPage++;
            }

            // Write to CSV
            
            h1Text = h1Text.Trim().Replace(" ", "_").Replace("/", "-");
            string fileName = $"{h1Text}.csv";
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "ListOfProductLinksFiles/", fileName);
            var lines = new List<string> { "Page,URL" };

            foreach (var (pageNum, url) in productLinks)
            {
                string escapedUrl = url.Replace("\"", "\"\"");
                lines.Add($"{pageNum},\"{escapedUrl}\"");
            }

            File.WriteAllLines(csvPath, lines, Encoding.UTF8);

            ScrapedCategoryLinksTracer.WriteInTheScrapedLinksList(linkToExport);

            Console.WriteLine($"✅ Done. {productLinks.Count} entries written to {csvPath}");
        }
        
    }
}
