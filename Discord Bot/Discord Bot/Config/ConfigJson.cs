using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tiny_Bot
{
    class ConfigJson
    {
        [JsonProperty("Token")]
        public string Token { get; private set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; private set; }
    }
}
