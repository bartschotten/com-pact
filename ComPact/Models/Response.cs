using Newtonsoft.Json;

namespace ComPact.Models
{
    internal class Response
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("headers")]
        public Headers Headers { get; set; } = new Headers();
        [JsonProperty("body")]
        public dynamic Body { get; set; }
        [JsonProperty("matchingRules")]
        internal MatchingRuleCollection MatchingRules { get; set; }
    }
}
