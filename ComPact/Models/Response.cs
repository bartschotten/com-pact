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

        internal Response() { }

        internal Response(ResponseV2 responseV2)
        {
            if (responseV2 == null)
            {
                throw new System.ArgumentNullException(nameof(responseV2));
            }

            Status = responseV2.Status;
            Headers = responseV2.Headers;
            Body = responseV2.Body;
            MatchingRules = new MatchingRuleCollection(responseV2.MatchingRules);
        }
    }
}
