using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models
{
    public class MatcherList
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

        internal List<string> Match(object expectedValue, object actualValue)
        {
            return Match(JToken.FromObject(expectedValue), JToken.FromObject(actualValue));
        }

        internal object First()
        {
            throw new NotImplementedException();
        }
    }
}
