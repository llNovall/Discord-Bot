using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

public class Nanogif
{
    [JsonConstructor]
    public Nanogif(
        [JsonProperty("dims")] List<int> dims,
        [JsonProperty("url")] string url,
        [JsonProperty("size")] int size,
        [JsonProperty("preview")] string preview
    )
    {
        Dims = dims;
        Url = url;
        Size = size;
        Preview = preview;
    }

    [JsonProperty("dims")]
    public readonly List<int> Dims;

    [JsonProperty("url")]
    public readonly string Url;

    [JsonProperty("size")]
    public readonly int Size;

    [JsonProperty("preview")]
    public readonly string Preview;
}