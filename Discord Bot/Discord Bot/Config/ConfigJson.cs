using Newtonsoft.Json;

namespace Discord_Bot.Config
{
    internal class ConfigJson
    {
        [JsonProperty("Token")]
        public string Token { get; private set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; private set; }
    }
}