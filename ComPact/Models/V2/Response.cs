using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models.V2
{
    public class Response
    {
        [JsonProperty("status")]
        public int Status { get; set; } = 200;
        [JsonProperty("headers")]
        public Headers Headers { get; set; } = new Headers();
        [JsonProperty("body")]
        public dynamic Body { get; set; }
        [JsonProperty("matchingRules")]
        internal Dictionary<string, Matcher> MatchingRules { get; set; } = new Dictionary<string, Matcher>();

        internal Response()
        {
        }

        internal void SetEmptyValuesToNull()
        {
            Headers = Headers.Any() ? Headers : null;
            MatchingRules = MatchingRules.Any() ? MatchingRules : null;
        }
    }
}
