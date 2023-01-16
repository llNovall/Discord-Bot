using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

public class Mp4
{
    [JsonConstructor]
    public Mp4(
        [JsonProperty("size")] int size,
        [JsonProperty("dims")] List<int> dims,
        [JsonProperty("duration")] double duration,
        [JsonProperty("url")] string url,
        [JsonProperty("preview")] string preview
    )
    {
        Size = size;
        Dims = dims;
        Duration = duration;
        Url = url;
        Preview = preview;
    }

    [JsonProperty("size")]
    public readonly int Size;

    [JsonProperty("dims")]
    public readonly List<int> Dims;

    [JsonProperty("duration")]
    public readonly double Duration;

    [JsonProperty("url")]
    public readonly string Url;

    [JsonProperty("preview")]
    public readonly string Preview;
}