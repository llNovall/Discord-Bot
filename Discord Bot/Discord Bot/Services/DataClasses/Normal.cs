using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.DataClasses;

public class Normal
{
    [JsonProperty("avg")]
    public int? Avg;

    [JsonProperty("err")]
    public int Err;

    [JsonProperty("warn")]
    public bool Warn;

    [JsonProperty("closestRank")]
    public string ClosestRank;

    [JsonProperty("percentile")]
    public double? Percentile;

    [JsonProperty("timestamp")]
    public int? Timestamp;

    [JsonProperty("historical")]
    public List<Historical> Historical;
}