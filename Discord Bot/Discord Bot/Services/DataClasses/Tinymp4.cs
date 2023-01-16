using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.DataClasses;

public class Tinymp4
{
    [JsonConstructor]
    public Tinymp4(
        [JsonProperty("preview")] string preview,
        [JsonProperty("dims")] List<int> dims,
        [JsonProperty("url")] string url,
        [JsonProperty("duration")] double duration,
        [JsonProperty("size")] int size
    )
    {
        Preview = preview;
        Dims = dims;
        Url = url;
        Duration = duration;
        Size = size;
    }

    [JsonProperty("preview")]
    public readonly string Preview;

    [JsonProperty("dims")]
    public readonly List<int> Dims;

    [JsonProperty("url")]
    public readonly string Url;

    [JsonProperty("duration")]
    public readonly double Duration;

    [JsonProperty("size")]
    public readonly int Size;
}