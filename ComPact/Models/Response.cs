using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComPact.Models
{
    public class Response
    {
        private bool _matcherFound;

        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("headers")]
        public Headers Headers { get; set; }
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
                if (token.Value<string>().StartsWith("Pact."))
                {
                    var path = $"$.{token.Parent.Parent.Path}";
                    var inlineMatchingRule = ((JObject)token.Parent.Parent);
                    if (token.Value<string>() == "Pact.SomethingLike")
                    {
                        token.Root["matchingRules"][path] = JObject.FromObject(new { match = "type" });
                    }
                    else if (token.Value<string>() == "Pact.ArrayLike")
                    {
                        path += "[*]";
                        var minValue = inlineMatchingRule["Min"].Value<string>();
                        token.Root["matchingRules"][path] = JObject.FromObject(new { match = "type", min = minValue });
                    }
                    else if (token.Value<string>() == "Pact.Term")
                    {
                        var regexValue = inlineMatchingRule["Regex"].Value<string>();
                        token.Root["matchingRules"][path] = JObject.FromObject(new { match = "regex", regex = regexValue });
                    }
                    token.Parent.Parent.Replace(inlineMatchingRule["Example"]);
                    _matcherFound = true;
                }
            }
            else
            {
                if (token.Children().Count() == 1)
                {
                    ParseToken(token.Children().First());
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
}
