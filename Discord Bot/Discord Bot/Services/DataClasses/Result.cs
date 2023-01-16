using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

public class Result
{
    [JsonConstructor]
    public Result(
        [JsonProperty("id")] string id,
        [JsonProperty("title")] string title,
        [JsonProperty("h1_title")] string h1Title,
        [JsonProperty("media")] List<Medium> media,
        [JsonProperty("bg_color")] string bgColor,
        [JsonProperty("created")] double created,
        [JsonProperty("itemurl")] string itemurl,
        [JsonProperty("url")] string url,
        [JsonProperty("tags")] List<object> tags,
        [JsonProperty("flags")] List<object> flags,
        [JsonProperty("shares")] int shares,
        [JsonProperty("hasaudio")] bool hasaudio,
        [JsonProperty("hascaption")] bool hascaption,
        [JsonProperty("source_id")] string sourceId,
        [JsonProperty("composite")] object composite
    )
    {
        Id = id;
        Title = title;
        H1Title = h1Title;
        Media = media;
        BgColor = bgColor;
        Created = created;
        Itemurl = itemurl;
        Url = url;
        Tags = tags;
        Flags = flags;
        Shares = shares;
        Hasaudio = hasaudio;
        Hascaption = hascaption;
        SourceId = sourceId;
        Composite = composite;
    }

    [JsonProperty("id")]
    public readonly string Id;

    [JsonProperty("title")]
    public readonly string Title;

    [JsonProperty("h1_title")]
    public readonly string H1Title;

    [JsonProperty("media")]
    public readonly List<Medium> Media;

    [JsonProperty("bg_color")]
    public readonly string BgColor;

    [JsonProperty("created")]
    public readonly double Created;

    [JsonProperty("itemurl")]
    public readonly string Itemurl;

    [JsonProperty("url")]
    public readonly string Url;

    [JsonProperty("tags")]
    public readonly List<object> Tags;

    [JsonProperty("flags")]
    public readonly List<object> Flags;

    [JsonProperty("shares")]
    public readonly int Shares;

    [JsonProperty("hasaudio")]
    public readonly bool Hasaudio;

    [JsonProperty("hascaption")]
    public readonly bool Hascaption;

    [JsonProperty("source_id")]
    public readonly string SourceId;

    [JsonProperty("composite")]
    public readonly object Composite;
}