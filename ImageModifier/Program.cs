using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string path = "data.csv";

        // Check if the file exists. If not, create it with a header and initial data.
        if (!File.Exists(path))
        {
            List<string> initialLines = new List<string>
            {
                "Id,Name,Email",
                "1,John Doe,john@example.com",
                "2,Jane Smith,jane@example.com"
            };
            File.WriteAllLines(path, initialLines);
            Console.WriteLine("Initial CSV file created.");
        }

        //Lines to append
        List<string> newLines = new List<string>
        {
            "3,Bob Marley,bob@example.com",
            "4,Alice Johnson,alice@example.com"
        };

        // Append the new lines
        File.AppendAllLines(path, newLines);
        Console.WriteLine("New lines appended to CSV file.");

        // Show the full contents of the file
        Console.WriteLine("\nCurrent contents of the file:");
        string[] allLines = File.ReadAllLines(path);
        foreach (var line in allLines)
        {
            Console.WriteLine(line);
        }
    }
}
