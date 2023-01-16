using System.Collections.Generic;
using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses;

public class Ranked
{
    [JsonProperty("avg")]
    public int? Avg;

    [JsonProperty("err")]
    public int Err;

    [JsonProperty("warn")]
    public bool Warn;

    [JsonProperty("summary")]
    public string Summary;

    [JsonProperty("closestRank")]
    public string ClosestRank;

    [JsonProperty("percentile")]
    public double? Percentile;

    [JsonProperty("tierData")]
    public List<object> TierData;

    [JsonProperty("timestamp")]
    public int? Timestamp;

    [JsonProperty("historical")]
    public List<Historical> Historical;

    [JsonProperty("historicalTierData")]
    public List<object> HistoricalTierData;
}