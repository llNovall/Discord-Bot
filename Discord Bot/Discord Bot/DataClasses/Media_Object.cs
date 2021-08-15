using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tiny_Bot.DataClasses
{
    [JsonObject]
    public class Media_Object
    {
        [JsonProperty("preview")]
        public string preview { get; set; }
        [JsonProperty("url")]
        public string url { get; set; }
        [JsonProperty("dims")]
        public int[] dims { get; set; }
        [JsonProperty("size")]
        public int size { get; set; }
    }
}
