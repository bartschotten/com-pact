using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models.V3
{
    internal class Message
    {
        [JsonProperty("providerStates")]
        internal List<ProviderState> ProviderStates { get; set; }
        [JsonProperty("description")]
        internal string Description { get; set; } = string.Empty;
        [JsonProperty("contents")]
        internal object Contents { get; set; }
        [JsonProperty("matchingRules")]
        internal MatchingRuleCollection MatchingRules { get; set; }
        [JsonProperty("metaData")]
        internal object Metadata { get; set; } = new { ContentType = "application/json" };

        internal List<string> Match(object actualMessage)
        {
            return Body.Match(Contents, actualMessage, MatchingRules);
        }

        internal void SetEmptyValuesToNull()
        {
            ProviderStates = ProviderStates?.FirstOrDefault() != null ? ProviderStates : null;
            if (MatchingRules != null)
            {
                MatchingRules.SetEmptyValuesToNull();
            }
        }
    }
}
