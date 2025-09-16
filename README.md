# WebScraperBot

A .NET 8 console application for automated **3-step web scraping** of Agilent products using the [Playwright](https://playwright.dev/dotnet/) framework.  
This tool was built to gather product information, metadata, and images from thousands of Agilent product pages.

Traditional static scraping frameworks like **HtmlAgilityPack** were not sufficient, since much of the product data was loaded dynamically. Playwright was chosen for its ability to handle dynamic content reliably.

---

## 📂 Solution Structure

The solution contains 5 projects:

- **ExtractLinks**  
  Scrapes product links from category pages on the Agilent website and saves them into CSV files (one per category).

- **ReadProductInfo**  
  Reads the saved CSV link files and scrapes detailed product information from each product page. Saves results to CSV files.

- **ImageScraper**  
  Downloads product images from each product page and saves them in category folders, named by their product part-numbers.

- **FolderLoopTest** & **CsvRowComparer**    
  Utility projects for post-scraping validation tasks.

---

## 🚀 How It Works

The scraping process runs in **three main steps**:

1. **Extract product links**  
   - Input: Category web pages from the Agilent website  
   - Output: CSV files containing product links per category  

2. **Scrape product info**  
   - Input: Product link CSVs from step 1  
   - Output: CSV files with detailed product data (as required by the employer)  

3. **Download product images**  
   - Input: Product link CSVs from step 1  
   - Output: Folders with product images, organized by product part-number  

---

## 📦 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (or any IDE with C# support)
- [Playwright for .NET](https://playwright.dev/dotnet/) (installed via NuGet)
