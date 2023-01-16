using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

public class Historical
{
    [JsonProperty("avg")]
    public int Avg;

    [JsonProperty("err")]
    public int Err;

    [JsonProperty("warn")]
    public bool Warn;

    [JsonProperty("timestamp")]
    public int Timestamp;
}