using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.DataClasses;

public class Nanowebm
{
    [JsonConstructor]
    public Nanowebm(
        [JsonProperty("preview")] string preview,
        [JsonProperty("url")] string url,
        [JsonProperty("size")] int size,
        [JsonProperty("dims")] List<int> dims
    )
    {
        Preview = preview;
        Url = url;
        Size = size;
        Dims = dims;
    }

    [JsonProperty("preview")]
    public readonly string Preview;

    [JsonProperty("url")]
    public readonly string Url;

    [JsonProperty("size")]
    public readonly int Size;

    [JsonProperty("dims")]
    public readonly List<int> Dims;
}