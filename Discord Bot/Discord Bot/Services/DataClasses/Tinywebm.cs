using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

public class Tinywebm
{
    [JsonConstructor]
    public Tinywebm(
        [JsonProperty("size")] int size,
        [JsonProperty("preview")] string preview,
        [JsonProperty("url")] string url,
        [JsonProperty("dims")] List<int> dims
    )
    {
        Size = size;
        Preview = preview;
        Url = url;
        Dims = dims;
    }

    [JsonProperty("size")]
    public readonly int Size;

    [JsonProperty("preview")]
    public readonly string Preview;

    [JsonProperty("url")]
    public readonly string Url;

    [JsonProperty("dims")]
    public readonly List<int> Dims;
}