using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComPact.Models.V3
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

        internal List<string> Match(object actualMessage)
        {
            return Body.Match(Content, actualMessage, MatchingRules);
        }
    }
}
