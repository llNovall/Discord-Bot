using Newtonsoft.Json;

namespace Discord_Bot.Services.DataClasses
{
    public class WhatISMyMMRData
    {
        [JsonProperty("ranked")]
        public Ranked Ranked;

        [JsonProperty("normal")]
        public Normal Normal;

        [JsonProperty("ARAM")]
        public Aram ARAM;
    }
}