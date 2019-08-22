using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace ComPact.Models.V3
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

        internal Response(V2.Response responseV2)
        {
            if (responseV2 == null)
            {
                throw new System.ArgumentNullException(nameof(responseV2));
            }

            Status = responseV2.Status;
            Headers = responseV2.Headers;
            Body = responseV2.Body;
            if (responseV2.MatchingRules != null)
            {
                MatchingRules = new MatchingRuleCollection(responseV2.MatchingRules);
            }
        }

        internal Response(IRestResponse restResponse)
        {
            if (restResponse == null)
            {
                throw new ArgumentNullException(nameof(restResponse));
            }

            Status = (int)restResponse?.StatusCode;
            Headers = new Headers(restResponse.Headers);
            Body = JsonConvert.DeserializeObject(restResponse.Content);
        }

        internal List<string> Match(Response actualResponse)
        {
            var differences = new List<string>();

            if (Status != actualResponse.Status)
            {
                differences.Add($"Expected status {Status}, but was {actualResponse.Status}");
            }

            differences.AddRange(Headers.Match(actualResponse.Headers, MatchingRules));

            differences.AddRange(Models.Body.Match(Body, actualResponse.Body, MatchingRules));

            return differences;
        }
    }
}
