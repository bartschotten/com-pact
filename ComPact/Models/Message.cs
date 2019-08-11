using Newtonsoft.Json;

namespace ComPact.Models
{
    internal class Message
    {
        [JsonProperty("providerState")]
        internal ProviderState ProviderState { get; set; }
        [JsonProperty("description")]
        internal string Description { get; set; }
        [JsonProperty("content")]
        internal object Content { get; set; }
        [JsonProperty("matchingRules")]
        internal MatchingRuleCollection MatchingRules { get; set; }
        [JsonProperty("metaData")]
        internal object Metadata { get; set; } = new { ContentType = "application/json" };
    }
}
