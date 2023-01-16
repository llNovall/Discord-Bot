using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

public class Nanomp4
{
    [JsonConstructor]
    public Nanomp4(
        [JsonProperty("size")] int size,
        [JsonProperty("url")] string url,
        [JsonProperty("duration")] double duration,
        [JsonProperty("preview")] string preview,
        [JsonProperty("dims")] List<int> dims
    )
    {
        Size = size;
        Url = url;
        Duration = duration;
        Preview = preview;
        Dims = dims;
    }

    [JsonProperty("size")]
    public readonly int Size;

    [JsonProperty("url")]
    public readonly string Url;

    [JsonProperty("duration")]
    public readonly double Duration;

    [JsonProperty("preview")]
    public readonly string Preview;

    [JsonProperty("dims")]
    public readonly List<int> Dims;
}