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

        internal bool TryGetApplicableMatcherListForToken(JToken token, out MatcherList matcherList)
        {
            var matchingPaths = new List<string>();
            foreach (var path in Body.Select(b => b.Key))
            {
                var matchingTokens = token.Root.SelectTokens(path).ToList();
                if (matchingTokens.Select(t => t.Path).Intersect(token.AncestorsAndSelf().Select(t => t.Path)).Any())
                {
                    matchingPaths.Add(path);
                }
            }

            if (!matchingPaths.Any())
            {
                matcherList = null;
                return false;
            }

            var orderedPaths = matchingPaths.OrderBy(m => m, new MatchingRulePathComparer()).ToList();
            matcherList = Body[orderedPaths.First()];
            return true;
        }

        private class MatchingRulePathComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var lengthComparison = x.Length.CompareTo(y.Length);
                if (lengthComparison == 0)
                {
                    return y.Count(c => c == '*').CompareTo(x.Count(c => c == '*'));
                }
                else
                {
                    return lengthComparison;
                }
            }
        }
    }
}
