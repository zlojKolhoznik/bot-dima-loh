using Newtonsoft.Json;

namespace DimaNahBot;

public struct CongratulationParameters
{
    [JsonProperty("gifUrl")] public string GifUrl { get; set; }

    [JsonProperty("text")] public string Text { get; set; }
}