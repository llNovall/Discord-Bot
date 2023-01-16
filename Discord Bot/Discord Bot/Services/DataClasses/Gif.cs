using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

public class Gif
{
    [JsonConstructor]
    public Gif(
        [JsonProperty("preview")] string preview,
        [JsonProperty("dims")] List<int> dims,
        [JsonProperty("url")] string url,
        [JsonProperty("size")] int size
    )
    {
        Preview = preview;
        Dims = dims;
        Url = url;
        Size = size;
    }

    [JsonProperty("preview")]
    public readonly string Preview;

    [JsonProperty("dims")]
    public readonly List<int> Dims;

    [JsonProperty("url")]
    public readonly string Url;

    [JsonProperty("size")]
    public readonly int Size;
}