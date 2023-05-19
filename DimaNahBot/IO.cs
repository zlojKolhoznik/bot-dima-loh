using System.Configuration;
using Newtonsoft.Json;

namespace DimaNahBot;

public static class IO
{
    static IO()
    {
        if (OperatingSystem.IsLinux())
        {
            DataFolderPath = ConfigurationManager.AppSettings["LinuxDataFolderPath"]!;
        } 
        else if (OperatingSystem.IsWindows())
        {
            DataFolderPath = ConfigurationManager.AppSettings["WindowsDataFolderPath"]!;
        }
    }
    
    public static string DataFolderPath { get; }
    
    public static T? TryReadObject<T>(string filePath)
    {
        var fi = new FileInfo(filePath);
        if (fi.Extension != ".json")
        {
            throw new FormatException("Extension of the file is not .json");
        }

        if (!fi.Exists)
        {
            throw new FileNotFoundException();
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
        }
        catch (Exception e)
        {
            return default;
        }
    }

    public static void SaveObject(object obj, string filePath)
    {
        File.WriteAllText(filePath, JsonConvert.SerializeObject(obj));
    }
    
    public static Dictionary<long, int> TryLoadFrequencies()
    {
        return TryReadObject<Dictionary<long, int>>($"{DataFolderPath}/frequencies.json") ??
               new Dictionary<long, int>();
    }

    public static void SaveFrequencies(Dictionary<long, int> frequencies)
    {
        SaveObject(frequencies, "frequencies.json");
    }

    public static Dictionary<string, CongratulationParameters> TryReadCalendar()
    {
        return TryReadObject<Dictionary<string, CongratulationParameters>>($"{DataFolderPath}/holidays.json") ??
               new Dictionary<string, CongratulationParameters>();
    }
}