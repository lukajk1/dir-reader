using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Text.Json;

namespace DirReader;
internal class Program
{
    const string fileName = "presets.json";
    static List<Preset>? presets = new List<Preset>();
    static void Main(string[] args)
    {
        ReadPresets();
        MainLoop();

    }
    static void ReadPresets()
    {
        //Console.WriteLine(File.ReadAllText(fileName));

        try
        {
            string jsonContent = File.ReadAllText(fileName);
            try
            {
                presets = JsonSerializer.Deserialize<List<Preset>>(jsonContent);
            }
            catch (JsonException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("\n(no existing presets found.)");
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    static void SavePresets()
    {
        string jsonString = JsonSerializer.Serialize(presets, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fileName, jsonString);
    }
    static void CountLines(Preset preset)
    {
        string[] excludedFolderNames = { "node_modules", "obj" };

        if (Directory.Exists(preset.Directory))
        {
            string[] processableFiles = Directory.GetFiles(preset.Directory, "*"+preset.ExtensionType, SearchOption.AllDirectories);
            int totalLineCount = 0;

            foreach (var file in processableFiles)
            {
                // Get the directory path of the file
                string fileDirectory = Path.GetDirectoryName(file);

                // Check if any excluded folder name is part of the directory path
                if (excludedFolderNames.Any(folder => fileDirectory.Split(Path.DirectorySeparatorChar).Contains(folder)))
                {
                    continue; // Skip processing for this file
                }

                int lineCount = File.ReadAllLines(file).Length;
                totalLineCount += lineCount;
                Console.WriteLine($"{file}: {lineCount} lines");
            }

            Console.WriteLine($"Total lines in all {preset.ExtensionType} files: {totalLineCount}\n");
        }
        else
        {
            Console.WriteLine("The specified directory does not exist.");
        }
    }
    static void CountFiles(Preset preset)
    {
        int totalFiles = 0;
        if (Directory.Exists(preset.Directory))
        {
            string[] subDirectories = Directory.GetDirectories(preset.Directory);
            foreach (var subDirectory in subDirectories)
            {
                string[] files = Directory.GetFiles(subDirectory, "*"+preset.ExtensionType, SearchOption.AllDirectories);
                totalFiles += files.Length;
                Console.WriteLine($"Folder: {subDirectory} contains {files.Length} {preset.ExtensionType} files");
            }
            Console.WriteLine($"\nTotal image count: {totalFiles}");
        }
        else
        {
            Console.WriteLine("The specified directory does not exist.");
        }
    }
    static void CountDir(bool saveToPreset)
    {
        string dir;
        string extension;
        string typeResponse;
        int type;

        Console.WriteLine("input directory:");
        dir = Console.ReadLine();

        Console.WriteLine("input extension (omit the .):");
        extension = Console.ReadLine();
        extension = "." + extension; 

        Console.WriteLine("what should be counted?");
        Console.WriteLine("1: number of lines");
        Console.WriteLine("2: number of files");
        typeResponse = Console.ReadLine();

        Preset? newPreset = null;

        if (int.TryParse(typeResponse, out int result))
        {
            type = result;
            switch (type)
            {
                case 1:
                    newPreset = new Preset(extension, dir, Preset.SearchType.LineCount);
                    CountLines(newPreset);
                    break;
                case 2:
                    newPreset = new Preset(extension, dir, Preset.SearchType.FileCount);
                    CountFiles(newPreset);
                    break;
                default:
                    break;
            }
        }
        else
        {
            return;
        }

        if (saveToPreset && newPreset != null) {
            presets.Add(newPreset);
            SavePresets();
        }
    }
    static void MainLoop()
    {
        while (true)
        {
            const int offset = 9;
            int optionIterator = 1;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("options:");
            Console.WriteLine($"{"c:",-offset} read from a directory");
            Console.WriteLine($"{"n:",-offset} read from a directory and save to presets");
            Console.WriteLine($"{"-d x:",-offset} deletes xth preset");
            Console.WriteLine($"{"q:",-offset} exit");
            Console.WriteLine();

            Console.WriteLine("presets:");

            if (presets.Count > 0)
            {
                foreach (Preset preset in presets)
                {
                    string optionText;

                    if (preset.Type == Preset.SearchType.LineCount)
                    {
                        optionText = $"{optionIterator++}:";
                        Console.WriteLine($"{optionText,-offset} read number of lines in {preset.ExtensionType} files in dir {preset.Directory}");
                    }
                    else if (preset.Type == Preset.SearchType.FileCount)
                    {
                        optionText = $"{optionIterator++}:";
                        Console.WriteLine($"{optionText,-offset} read number of {preset.ExtensionType} files in dir {preset.Directory}");
                    }
                    else
                    {
                        Console.WriteLine("Unknown operation on preset");
                    }
                }
            }
            else
            {
                Console.WriteLine("(No presets found.)");
            }
            Console.ResetColor();

            string response = Console.ReadLine().ToLower();

            if (int.TryParse(response, out int result))
            {
                if (result - 1 >= 0 && result - 1 < presets.Count)
                {
                    if (presets[result-1].Type == Preset.SearchType.FileCount)
                    {
                        CountFiles(presets[result - 1]);
                    }
                    else if (presets[result - 1].Type == Preset.SearchType.LineCount)
                    {
                        CountLines(presets[result - 1]);
                    }
                }
            }
            else if (response.StartsWith("-d "))
            {
                string valuePart = response.Substring(3); 

                if (int.TryParse(valuePart, out int parsedValue))
                {
                    presets.RemoveAt(parsedValue - 1);
                    Console.WriteLine($"Removed preset {parsedValue}");
                    SavePresets();
                }
                else
                {
                    Console.WriteLine("That preset does not exist.");
                }
            }
            else if (response == "q")
            {
                break;
            }
            else if (response == "c")
            {
                CountDir(false);
            }
            else if (response == "n")
            {
                CountDir(true);
            }
            else
            {
                Console.WriteLine("could not parse input. please select an option:\n");
            }

        }
    }
}

