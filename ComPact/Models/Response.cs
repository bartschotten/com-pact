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
        internal JObject MatchingRules { get; set; } = new JObject();

        internal Response ConvertMatchingRules()
        {
            var jobj = JObject.Parse(JsonConvert.SerializeObject(this));

            do
            {
                _matcherFound = false;
                FindAndConvertFirstUnconvertedMatchingRule(jobj.Root);
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
        private void FindAndConvertFirstUnconvertedMatchingRule(JToken token)
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
                        var minValue = inlineMatchingRule["Min"].Value<int>();
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
                    FindAndConvertFirstUnconvertedMatchingRule(token.Children().First());
                }
                else
                {
                    foreach (var child in token.Children())
                    {
                        FindAndConvertFirstUnconvertedMatchingRule(child);
                    }
                }
            }
        }

        internal List<string> Match(Response actualResponse)
        {
            var expectedJObj = JObject.Parse(JsonConvert.SerializeObject(this));
            expectedJObj.Remove("matchingRules");
            var actualJObj = JObject.Parse(JsonConvert.SerializeObject(actualResponse));

            var differences = new List<string>();
            CompareTokenAndItsChildren(expectedJObj.Root, actualJObj, differences);

            return differences;
        }

        private void CompareTokenAndItsChildren(JToken token, JObject actualJObject, List<string> differences)
        {
            string difference = null;
            switch(token.Type)
            {
                case JTokenType.String:
                    difference = CompareExpectedTokenWithActual<string>(token, actualJObject);
                    break;
                case JTokenType.Integer:
                    difference = CompareExpectedTokenWithActual<int>(token, actualJObject);
                    break;
                case JTokenType.Float:
                    difference = CompareExpectedTokenWithActual<double>(token, actualJObject);
                    break;
                case JTokenType.Boolean:
                    difference = CompareExpectedTokenWithActual<bool>(token, actualJObject);
                    break;
                case JTokenType.Array:
                    difference = CompareExpectedArrayWithActual(token, actualJObject);
                    break;
            }
            if (difference != null)
            {
                differences.Add(difference);
            }

            if (token.Children().Count() == 1)
            {
                CompareTokenAndItsChildren(token.Children().First(), actualJObject, differences);
            }
            else
            {
                foreach (var child in token.Children())
                {
                    CompareTokenAndItsChildren(child, actualJObject, differences);
                }
            }
        }

        private string CompareExpectedTokenWithActual<T>(JToken expectedToken, JObject actualJObject) where T: IEquatable<T>
        {
            var expectedValue = expectedToken.Value<T>();
            var actualToken = actualJObject.SelectToken(expectedToken.Path);
            if (actualToken == null)
            {
                return $"Property {expectedToken.Path} was not present in the actual response.";
            }
            else
            {
                var matchingRule = GetMatchingRuleForToken(expectedToken);
                var regexValue = matchingRule?["regex"]?.Value<string>();
                var actualValue = actualToken.Value<T>();
                if (matchingRule?["match"]?.Value<string>() == "type")
                {
                    if (actualToken.Type != expectedToken.Type)
                    {
                        return $"Expected value of type {expectedToken.Type} (like: {expectedValue}) at {expectedToken.Path}, but was value of type {actualToken.Type}.";
                    }
                }
                else if (regexValue != null)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(actualToken.Value<string>(), regexValue))
                    {
                        return $"Expected value matching {regexValue} (like: {expectedValue}) at {expectedToken.Path}, but was {actualValue}.";
                    }
                }
                else if (!actualValue.Equals(expectedValue))
                {
                    return $"Expected {expectedValue} at {expectedToken.Path}, but was {actualValue}.";
                }
            }

            return null;
        }

        private string CompareExpectedArrayWithActual(JToken expectedToken, JObject actualJObject)
        {
            var actualToken = actualJObject.SelectToken(expectedToken.Path);
            if (actualToken == null)
            {
                return $"Array {expectedToken.Path} was not present in the actual response.";
            }
            var matchingRule = GetMatchingRuleForToken(expectedToken);
            var expectedMinItems = matchingRule?["min"]?.Value<int>();
            var actualItemsCount = actualToken.Children().Count();
            if (expectedMinItems.HasValue && actualItemsCount < expectedMinItems)
            {
                return $"Expected an array with {expectedMinItems} item(s) at {expectedToken.Path}, but was {actualItemsCount} items(s).";
            }

            return null;
        }

        private JToken GetMatchingRuleForToken(JToken token)
        {
            var currentToken = token;
            while (currentToken.Root != currentToken)
            {
                if (MatchingRules.TryGetValue("$." + currentToken.Path, out var matchingRuleToken))
                {
                    return matchingRuleToken;
                }
                if (MatchingRules.TryGetValue("$." + currentToken.Path + "[*]", out matchingRuleToken))
                {
                    return matchingRuleToken;
                }
                currentToken = currentToken.Parent;
            }
            return null;
        }
    }
}
