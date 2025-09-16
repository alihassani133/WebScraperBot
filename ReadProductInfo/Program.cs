using System.Threading.Tasks;
using Microsoft.Playwright;
using ReadProductInfo;

class Program
{
    static async Task Main()
    {

        if (!Directory.Exists("FinalProductLists"))
            Directory.CreateDirectory("FinalProductLists");

        string outputFolderPath = @"FinalProductLists/";

        string[] csvFiles = Directory.GetFiles(@"ListOfProductLinksFiles/", "*.csv");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        int rowNumber = 0;
        foreach (string csvFile in csvFiles)
        {
            rowNumber = 1;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(csvFile);
            string newFolderPath = Path.Combine(outputFolderPath, fileNameWithoutExtension);

            if (!Directory.Exists(newFolderPath))
            {
                Directory.CreateDirectory(newFolderPath);
                Console.WriteLine($"Create folder: {newFolderPath}");
            }
            else
            {
                Console.WriteLine($"Folder already exists: {newFolderPath}");
            }

            string outputCsvPath = Path.Combine(newFolderPath, "FinalProductList.csv");
            var scraper = new ProductScraper(outputCsvPath);

            var links = CsvManager.ReadLinks(csvFile);
            foreach (var link in links)
            {
                if (ScrapedLinksTracker.IsTheLinkScraped(link))
                {
                    Console.WriteLine($"Skipping (already scraped): {link}");
                    rowNumber++;
                    continue;
                }

                string fullUrl = "https://www.agilent.com" + link;
                Console.WriteLine($"Processing Product {rowNumber}: {fullUrl}");

                var product = await scraper.ScrapeProductAsync(page, fullUrl, rowNumber);

                CsvManager.AppendProductToCsv(product, outputCsvPath);

                // Mark as successfully scraped
                ScrapedLinksTracker.WriteInTheScrapedLinksList(link);
                rowNumber++;
            }
        }

        Console.WriteLine("✅ Done.");
    }
}
