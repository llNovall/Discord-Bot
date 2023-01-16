using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

public class Loopedmp4
{
    [JsonConstructor]
    public Loopedmp4(
        [JsonProperty("duration")] double duration,
        [JsonProperty("preview")] string preview,
        [JsonProperty("url")] string url,
        [JsonProperty("size")] int size,
        [JsonProperty("dims")] List<int> dims
    )
    {
        Duration = duration;
        Preview = preview;
        Url = url;
        Size = size;
        Dims = dims;
    }

    [JsonProperty("duration")]
    public readonly double Duration;

    [JsonProperty("preview")]
    public readonly string Preview;

    [JsonProperty("url")]
    public readonly string Url;

    [JsonProperty("size")]
    public readonly int Size;

    [JsonProperty("dims")]
    public readonly List<int> Dims;
}