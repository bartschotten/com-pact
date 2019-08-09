using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models
{
    public class Response
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("headers")]
        public Headers Headers { get; set; } = new Headers();
        [JsonProperty("body")]
        public dynamic Body { get; set; }
        [JsonProperty("matchingRules")]
        internal Dictionary<string, MatchingRule> MatchingRules { get; set; } = new Dictionary<string, MatchingRule>();

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
                var regexValue = matchingRule?.Regex;
                var actualValue = actualToken.Value<T>();
                if (matchingRule?.Match == "type")
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
            var expectedMinItems = matchingRule?.Min;
            var actualItemsCount = actualToken.Children().Count();
            if (expectedMinItems.HasValue && actualItemsCount < expectedMinItems)
            {
                return $"Expected an array with {expectedMinItems} item(s) at {expectedToken.Path}, but was {actualItemsCount} items(s).";
            }

            return null;
        }

        private MatchingRule GetMatchingRuleForToken(JToken token)
        {
            var currentToken = token;
            while (currentToken.Root != currentToken)
            {
                if (MatchingRules.TryGetValue("$." + currentToken.Path, out var rule))
                {
                    return rule;
                }
                if (MatchingRules.TryGetValue("$." + currentToken.Path + "[*]", out rule))
                {
                    return rule;
                }
                currentToken = currentToken.Parent;
            }
            return null;
        }
    }
}
