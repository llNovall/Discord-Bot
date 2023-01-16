using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

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
        Webm = webm;
        Gif = gif;
        Nanogif = nanogif;
        Tinygif = tinygif;
        Tinywebm = tinywebm;
        Mediumgif = mediumgif;
        Tinymp4 = tinymp4;
        Mp4 = mp4;
        Nanowebm = nanowebm;
        Nanomp4 = nanomp4;
        Loopedmp4 = loopedmp4;
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