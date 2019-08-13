using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models
{
    internal class MatchingRuleCollection
    {
        [JsonProperty("header")]
        internal Dictionary<string, MatcherList> Header { get; set; }
        [JsonProperty("body")]
        internal Dictionary<string, MatcherList> Body { get; set; }

        internal MatchingRuleCollection() { }

        internal MatchingRuleCollection(Dictionary<string, Matcher> matchingRules)
        {
            if (matchingRules == null)
            {
                throw new System.ArgumentNullException(nameof(matchingRules));
            }

            Body = new Dictionary<string, MatcherList>();
            foreach (var rule in matchingRules.Where(m => m.Key.StartsWith("$.body")))
            {
                Body.Add(rule.Key.Substring(7), new MatcherList { Matchers = new List<Matcher> { rule.Value } });
            }

            Header = new Dictionary<string, MatcherList>();
            foreach (var rule in matchingRules.Where(m => m.Key.StartsWith("$.headers")))
            {
                Header.Add(rule.Key.Substring(10), new MatcherList { Matchers = new List<Matcher> { rule.Value } });
            }
        }
    }

    internal class MatcherList
    {
        [JsonProperty("combine")]
        internal string Combine { get; set; } = "AND";
        [JsonProperty("matchers")]
        internal List<Matcher> Matchers { get; set; }

        internal List<string> Match(JToken expectedToken, JToken actualToken)
        {
            var differences = new List<string>();

            var anySuccessfulMatchers = false;
            foreach (var matcher in Matchers)
            {
                var matcherDifferences = matcher.Match(expectedToken, actualToken);
                if (matcherDifferences.Any())
                {
                    differences.AddRange(matcherDifferences);
                }
                else
                {
                    anySuccessfulMatchers = true;
                }
            }

            if ((Combine == "AND" && differences.Any()) || !anySuccessfulMatchers)
            {
                return differences;
            }
            return new List<string>();
        }

        internal List<string> Match<T>(object expectedValue, object actualValue)
        {
            return Match<T>(JToken.FromObject(expectedValue), JToken.FromObject(actualValue));
        }
    }
}
