using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DirReader;
internal class Program
{
    const string fileName = "presets.json";
    static List<Preset>? presets;
    static void Main(string[] args)
    {
        ReadPresets();

        Console.WriteLine("\nThis program reads the line count of all .cs files in a folder directory and its subfolders.\n");

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
        int totalImages = 0;
        if (Directory.Exists(preset.Directory))
        {
            string[] subDirectories = Directory.GetDirectories(preset.Directory);
            foreach (var subDirectory in subDirectories)
            {
                string[] pngFiles = Directory.GetFiles(subDirectory, preset.ExtensionType, SearchOption.AllDirectories);
                totalImages += pngFiles.Length;
                Console.WriteLine($"Folder: {subDirectory} contains {pngFiles.Length} .png files");
            }
            Console.WriteLine($"\nTotal image count: {totalImages}");
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
            int optionIterator = 1;
            Console.WriteLine($"options:");
            Console.WriteLine($"- c: read from a directory");
            Console.WriteLine($"- n: read from a directory and save to presets");
            Console.WriteLine($"");

            Console.WriteLine("(presets)");
            if (presets.Count > 0)
            {
                foreach (Preset preset in presets)
                {
                    Console.WriteLine($"- {optionIterator++}: read {preset.ExtensionType} files in dir {preset.Directory}");
                }
            }
            else
            {
                Console.WriteLine("(no presets found.)");
            }

            Console.WriteLine($"- q: exit");

            string response = Console.ReadLine().ToLower();

            if (response == "q")
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

