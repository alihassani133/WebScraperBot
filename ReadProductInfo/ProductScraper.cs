using Microsoft.Playwright;

namespace ReadProductInfo
{

    public class ProductScraper
    {
        private readonly string _imageBaseDir;
        private readonly string _csvPath;

        public ProductScraper(string imageBaseDir, string csvPath)
        {
            _imageBaseDir = imageBaseDir;
            _csvPath = csvPath;
            CsvManager.EnsureCsvHeader(csvPath);
        }

        public ProductScraper(string csvPath)
        {
            _csvPath = csvPath;
            CsvManager.EnsureCsvHeader(csvPath);
        }

        public async Task<ProductInfo> ScrapeProductAsync(IPage page, string url, int rowNumber)
        {
            var product = new ProductInfo
            {
                Number = rowNumber,
                WebpageLink = url
            };

            page.SetDefaultTimeout(90000);

            try
            {
                await RetryHelper.RunWithRetryAsync(() => page.GotoAsync(url));
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Part Number Group
                var partGroup = await RetryHelper.RunWithRetryAsync(() => page.QuerySelectorAsync("div.custom-partNumberGroup"));
                if (partGroup != null)
                {
                    product.PartNumberHtml = await RetryHelper.RunWithRetryAsync(() => partGroup.EvaluateAsync<string>("e => e.outerHTML")) ?? "Blank";
                    var partNo = await RetryHelper.RunWithRetryAsync(() => partGroup.QuerySelectorAsync("div.partNo"));
                    if (partNo != null)
                        product.PartNumber = await RetryHelper.RunWithRetryAsync(() => partNo.InnerTextAsync()) ?? $"Row{rowNumber}";
                    else
                        product.PartNumber = $"Row{rowNumber}";
                }
                else
                {
                    product.PartNumberHtml = "Blank";
                    product.PartNumber = $"Row{rowNumber}";
                }
                Console.WriteLine($"Product {rowNumber} Part-Number wrote successfully.");

                // Description
                //if (partGroup != null)
                //{
                //    var descElem = await RetryHelper.RunWithRetryAsync(() =>
                //        partGroup.EvaluateHandleAsync("node => node.nextElementSibling?.tagName === 'P' ? node.nextElementSibling : null"));
                //    if (descElem != null)
                //    {
                //        product.DescriptionHtml = await RetryHelper.RunWithRetryAsync(() =>
                //            descElem.AsElement().EvaluateAsync<string>("e => e.outerHTML")) ?? "Blank";
                //    }
                //}
                // ----------
                var descElem = await RetryHelper.RunWithRetryAsync(() =>
                    partGroup.EvaluateHandleAsync("node => node.nextElementSibling?.tagName === 'P' ? node.nextElementSibling : null"));
                if (descElem != null)
                {
                    product.DescriptionHtml = await RetryHelper.RunWithRetryAsync(() =>
                        descElem.AsElement().EvaluateAsync<string>("e => e.outerHTML")) ?? "Blank";
                }
                Console.WriteLine($"Product {rowNumber} Description wrote successfully.");

                // Price
                var priceElem = await RetryHelper.RunWithRetryAsync(() => page.QuerySelectorAsync("span.custom-price"));
                priceElem ??= await RetryHelper.RunWithRetryAsync(() => page.QuerySelectorAsync("span.custom-g"));
                if (priceElem is null)
                {
                    product.PriceHtml = "Blank";
                }
                else
                {
                    product.PriceHtml = await RetryHelper.RunWithRetryAsync(() => priceElem.EvaluateAsync<string>("e => e.outerHTML")) ?? "Blank";
                }
                Console.WriteLine($"Product {rowNumber} Price wrote successfully.");

                // Specifications
                var specElem = await RetryHelper.RunWithRetryAsync(() => page.QuerySelectorAsync("table.specsNewTable"));
                if (specElem is not null)
                {
                    product.SpecificationsHtml = await RetryHelper.RunWithRetryAsync(() => specElem.EvaluateAsync<string>("e => e.outerHTML")) ?? "";
                }
                else
                {
                    product.SpecificationsHtml = "Blank";
                }
                Console.WriteLine($"Product {rowNumber} Specifications wrote successfully.");


                // Kit Contents
                var kitContent = await RetryHelper.RunWithRetryAsync(() => page.QuerySelectorAsync("table.kitPnpTable"));
                if (kitContent is not null)
                {
                    product.KitContentHtml = await RetryHelper.RunWithRetryAsync(() => kitContent.EvaluateAsync<string>("e => e.outerHTML")) ?? "";
                }
                else
                {
                    product.KitContentHtml = "Blank";
                }
                Console.WriteLine($"Product {rowNumber} Kit Content wrote successfully.");

                //Image
               //var imgElem = await RetryHelper.RunWithRetryAsync(() => page.QuerySelectorAsync("div.media-object img"));
               // if (imgElem != null)
               // {
               //     var src = await RetryHelper.RunWithRetryAsync(() => imgElem.GetAttributeAsync("src"));
               //     if (!string.IsNullOrEmpty(src))
               //     {
               //         string imageUrl = "https://www.agilent.com" + src;
               //         string localPath = await ImageDownloader.DownloadImageWithPlaywrightAsync(page, imageUrl, _imageBaseDir, product.PartNumber);
               //         product.ImagePath = localPath;
               //         Console.WriteLine($"Product {rowNumber} Image downloaded successfully.");
               //     }
               // }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error scraping Product {rowNumber}: {ex.Message}");
            }

            return product;
        }
    }

}
