using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace ComPact.Models
{
    public class Response
    {
        private bool _matcherFound;

        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }
        [JsonProperty("body")]
        public dynamic Body { get; set; }
        [JsonProperty("matchingRules")]
        public JObject MatchingRules { get; set; } = new JObject();

        public Response ConvertMatchingRules()
        {
            var jobj = JObject.Parse(JsonConvert.SerializeObject(this));

            do
            {
                _matcherFound = false;
                ParseToken(jobj.Root);
            }
            while (_matcherFound);

            var convertedResponse = jobj.ToObject<Response>();
            return convertedResponse;
        }

        /// <summary>
        /// Recursively searches through a JObject tree from a root token to find and convert MatchingRules,
        /// but immediately returns when the first one is found, because the tree is rearranged as it is being searched.
        /// </summary>
        /// <param name="token"></param>
        private void ParseToken(JToken token)
        {
            if (_matcherFound)
            {
                return;
            }
            if (token.Type == JTokenType.String)
            {
                if (token.Value<string>() == "Pact::SomethingLike")
                {
                    var path = $"$.{token.Parent.Parent.Path}";
                    token.Root["matchingRules"][path] = JObject.Parse("{\"match\":\"type\"}");
                    token.Parent.Parent.Replace(token.Parent.Parent.Last.First);
                    _matcherFound = true;
                }
            }
            else
            {
                foreach (var child in token.Children())
                {
                    ParseToken(child);
                }
            }
        }
    }
}
