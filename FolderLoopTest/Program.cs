using System;
using System.IO;

class Program
{
    static void Main()
    {
        string folderPath = @"Lists/"; 

        string[] csvFiles = Directory.GetFiles(folderPath);

        foreach (var csvFile in csvFiles)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(csvFile);

            string newFolderPath = Path.Combine(folderPath, fileNameWithoutExtension);

            if (!Directory.Exists(newFolderPath))
            {
                Directory.CreateDirectory(newFolderPath);
                Console.WriteLine($"Create folder: {newFolderPath}");
            }
            else
            {
                Console.WriteLine($"Folder already exists: {newFolderPath}");
            }
        }

    }
}
