using ComPact.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ComPact.Models.V3
{
    internal class Response
    {
        [JsonProperty("status")]
        public int Status { get; set; } = 200;
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

        internal Response(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            Status = (int)response?.StatusCode;
            Headers = new Headers(response);
            try
            {
                Body = JsonConvert.DeserializeObject(response.Content?.ReadAsStringAsync().Result ?? string.Empty);
            }
            catch (JsonReaderException)
            {
                throw new PactException($"Response body could not be deserialized from JSON. Content-Type was {response.Content.Headers.ContentType}");
            }
        }

        internal List<string> Match(Response actualResponse)
        {
            var differences = new List<string>();

            if (Status != actualResponse.Status)
            {
                differences.Add($"Expected status {Status}, but was {actualResponse.Status}");
            }

            differences.AddRange(Headers.Match(actualResponse.Headers, MatchingRules));

            differences.AddRange(Models.BodyMatcher.Match(Body, actualResponse.Body, MatchingRules));

            return differences;
        }

        internal void SetEmptyValuesToNull()
        {
            Headers = Headers.Any() ? Headers : null;
            if (MatchingRules != null)
            {
                MatchingRules.SetEmptyValuesToNull();
            }
        }
    }
}
