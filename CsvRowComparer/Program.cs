using System;
using System.Globalization;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        string firstFolder = "ListOfProductLinksFiles";
        string secondBaseFolder = "FinalProductLists";

        if (!Directory.Exists(firstFolder) || !Directory.Exists(secondBaseFolder))
        {
            Console.WriteLine("One or both directories do not exist.");
            return;
        }

        var firstCsvFiles = Directory.GetFiles(firstFolder, "*.csv");

        var firstRowCounter = 0;
        var secondRowCounter = 0;
        foreach (var firstCsvFile in firstCsvFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(firstCsvFile);
            string secondCsvFile = Path.Combine(secondBaseFolder, fileName, "FinalProductList.csv");

            if (!File.Exists(secondCsvFile))
            {
                Console.WriteLine($"Missing file: {secondCsvFile}");
                continue;
            }

            int firstRowCount = CountCsvRows(firstCsvFile);
            int secondRowCount = CountCsvRows(secondCsvFile);

            Console.WriteLine($"File: {fileName}");
            Console.WriteLine($"  Rows in {firstCsvFile}: {firstRowCount}");
            Console.WriteLine($"  Rows in {secondCsvFile}: {secondRowCount}");
            Console.WriteLine($"  Match: {firstRowCount == secondRowCount}");
            Console.WriteLine();

            firstRowCounter += firstRowCount;
            secondRowCounter += secondRowCount;
        }
        Console.WriteLine(firstRowCounter);
        Console.WriteLine(secondRowCounter);
    }

    static int CountCsvRows(string filePath)
    {
        int count = 0;
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<dynamic>();
            count = records.Count();
            Console.WriteLine($"Number of rows: {count}");
        }
        return count;
    }
}
