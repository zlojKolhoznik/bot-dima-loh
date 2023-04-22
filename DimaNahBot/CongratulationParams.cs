using Newtonsoft.Json;

namespace DimaNahBot;

public struct CongratulationParams
{
    [JsonProperty("gifId")] public string GifId { get; set; }

    [JsonProperty("text")] public string Text { get; set; }
}