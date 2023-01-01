using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.DataClasses
{
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
            this.Preview = preview;
            this.Size = size;
            this.Dims = dims;
            this.Url = url;
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
            this.Preview = preview;
            this.Dims = dims;
            this.Url = url;
            this.Size = size;
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
            this.Dims = dims;
            this.Url = url;
            this.Size = size;
            this.Preview = preview;
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

    public class Tinygif
    {
        [JsonConstructor]
        public Tinygif(
            [JsonProperty("size")] int size,
            [JsonProperty("preview")] string preview,
            [JsonProperty("url")] string url,
            [JsonProperty("dims")] List<int> dims
        )
        {
            this.Size = size;
            this.Preview = preview;
            this.Url = url;
            this.Dims = dims;
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
            this.Size = size;
            this.Preview = preview;
            this.Url = url;
            this.Dims = dims;
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

    public class Mediumgif
    {
        [JsonConstructor]
        public Mediumgif(
            [JsonProperty("preview")] string preview,
            [JsonProperty("size")] int size,
            [JsonProperty("dims")] List<int> dims,
            [JsonProperty("url")] string url
        )
        {
            this.Preview = preview;
            this.Size = size;
            this.Dims = dims;
            this.Url = url;
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
            this.Preview = preview;
            this.Dims = dims;
            this.Url = url;
            this.Duration = duration;
            this.Size = size;
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
            this.Size = size;
            this.Dims = dims;
            this.Duration = duration;
            this.Url = url;
            this.Preview = preview;
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
            this.Preview = preview;
            this.Url = url;
            this.Size = size;
            this.Dims = dims;
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
            this.Size = size;
            this.Url = url;
            this.Duration = duration;
            this.Preview = preview;
            this.Dims = dims;
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
            this.Duration = duration;
            this.Preview = preview;
            this.Url = url;
            this.Size = size;
            this.Dims = dims;
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

    public class Medium
    {
        [JsonConstructor]
        public Medium(
            [JsonProperty("webm")] Webm webm,
            [JsonProperty("gif")] Gif gif,
            [JsonProperty("nanogif")] Nanogif nanogif,
            [JsonProperty("tinygif")] Tinygif tinygif,
            [JsonProperty("tinywebm")] Tinywebm tinywebm,
            [JsonProperty("mediumgif")] Mediumgif mediumgif,
            [JsonProperty("tinymp4")] Tinymp4 tinymp4,
            [JsonProperty("mp4")] Mp4 mp4,
            [JsonProperty("nanowebm")] Nanowebm nanowebm,
            [JsonProperty("nanomp4")] Nanomp4 nanomp4,
            [JsonProperty("loopedmp4")] Loopedmp4 loopedmp4
        )
        {
            this.Webm = webm;
            this.Gif = gif;
            this.Nanogif = nanogif;
            this.Tinygif = tinygif;
            this.Tinywebm = tinywebm;
            this.Mediumgif = mediumgif;
            this.Tinymp4 = tinymp4;
            this.Mp4 = mp4;
            this.Nanowebm = nanowebm;
            this.Nanomp4 = nanomp4;
            this.Loopedmp4 = loopedmp4;
        }

        [JsonProperty("webm")]
        public readonly Webm Webm;

        [JsonProperty("gif")]
        public readonly Gif Gif;

        [JsonProperty("nanogif")]
        public readonly Nanogif Nanogif;

        [JsonProperty("tinygif")]
        public readonly Tinygif Tinygif;

        [JsonProperty("tinywebm")]
        public readonly Tinywebm Tinywebm;

        [JsonProperty("mediumgif")]
        public readonly Mediumgif Mediumgif;

        [JsonProperty("tinymp4")]
        public readonly Tinymp4 Tinymp4;

        [JsonProperty("mp4")]
        public readonly Mp4 Mp4;

        [JsonProperty("nanowebm")]
        public readonly Nanowebm Nanowebm;

        [JsonProperty("nanomp4")]
        public readonly Nanomp4 Nanomp4;

        [JsonProperty("loopedmp4")]
        public readonly Loopedmp4 Loopedmp4;
    }

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
            this.Id = id;
            this.Title = title;
            this.H1Title = h1Title;
            this.Media = media;
            this.BgColor = bgColor;
            this.Created = created;
            this.Itemurl = itemurl;
            this.Url = url;
            this.Tags = tags;
            this.Flags = flags;
            this.Shares = shares;
            this.Hasaudio = hasaudio;
            this.Hascaption = hascaption;
            this.SourceId = sourceId;
            this.Composite = composite;
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

    public class TenorData
    {
        [JsonConstructor]
        public TenorData(
            [JsonProperty("results")] List<Result> results,
            [JsonProperty("next")] string next
        )
        {
            this.Results = results;
            this.Next = next;
        }

        [JsonProperty("results")]
        public readonly List<Result> Results;

        [JsonProperty("next")]
        public readonly string Next;
    }
}