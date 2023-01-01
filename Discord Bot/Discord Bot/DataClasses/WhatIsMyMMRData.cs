using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.DataClasses
{
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

    public class Historical
    {
        [JsonProperty("avg")]
        public int Avg;

        [JsonProperty("err")]
        public int Err;

        [JsonProperty("warn")]
        public bool Warn;

        [JsonProperty("timestamp")]
        public int Timestamp;
    }

    public class ARAM
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

    public class WhatISMyMMRData
    {
        [JsonProperty("ranked")]
        public Ranked Ranked;

        [JsonProperty("normal")]
        public Normal Normal;

        [JsonProperty("ARAM")]
        public ARAM ARAM;
    }
}