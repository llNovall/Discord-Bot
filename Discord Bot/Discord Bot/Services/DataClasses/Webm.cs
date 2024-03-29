﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.DataClasses;

public class Webm
{
    [JsonConstructor]
    public Webm(
        [JsonProperty("preview")] string preview,
        [JsonProperty("size")] int size,
        [JsonProperty("dims")] List<int> dims,
        [JsonProperty("url")] string url
    )
    {
        Preview = preview;
        Size = size;
        Dims = dims;
        Url = url;
    }

    [JsonProperty("preview")]
    public readonly string Preview;

    [JsonProperty("size")]
    public readonly int Size;

    [JsonProperty("dims")]
    public readonly List<int> Dims;

    [JsonProperty("url")]
    public readonly string Url;
}