using Newtonsoft.Json;

namespace DimaNahBot;

public static class IOTools
{
    public static Dictionary<long, int> TryLoadFrequencies()
    {
        try
        {
            return JsonConvert.DeserializeObject<Dictionary<long, int>>(File.ReadAllText("frequencies.json"));
        }
        catch (Exception)
        {
            return new Dictionary<long, int>();
        }
    }

    public static void SaveFrequencies(Dictionary<long, int> frequencies)
    {
        File.WriteAllText("frequencies.json", JsonConvert.SerializeObject(frequencies, Formatting.Indented));
    }

    public static Dictionary<string, CongratulationParams> TryReadCalendar()
    {
        try
        {
            return JsonConvert.DeserializeObject<Dictionary<string, CongratulationParams>>(
                File.ReadAllText("holidays.json"));
        }
        catch (Exception)
        {
            return new Dictionary<string, CongratulationParams>();
        }
    }
}