using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord_Bot.Services.DataClasses
{
    public class TenorData
    {
        [JsonConstructor]
        public TenorData(
            [JsonProperty("results")] List<Result> results,
            [JsonProperty("next")] string next
        )
        {
            Results = results;
            Next = next;
        }

        [JsonProperty("results")]
        public readonly List<Result> Results;

        [JsonProperty("next")]
        public readonly string Next;
    }
}