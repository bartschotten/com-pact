using ComPact.JsonHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models
{
    internal class Matcher
    {
        [JsonProperty("match")]
        [JsonConverter(typeof(StringEnumWithDefaultConverter))]
        internal MatcherType MatcherType { get; set; }
        [JsonProperty("min")]
        internal int? Min { get; set; }
        [JsonProperty("max")]
        internal int? Max { get; set; }
        [JsonProperty("regex")]
        internal string Regex { get; set; }
        [JsonProperty("value")]
        internal string Value { get; set; }

        internal List<string> Match(JToken expectedToken, JToken actualToken)
        {
            var differences = new List<string>();

            if (MatcherType == MatcherType.type && !expectedToken.IsSameJsonTypeAs(actualToken))
            {
                differences.Add($"Expected value of type {expectedToken.Type} (like: \'{expectedToken}\') at {expectedToken.Path}, but was value of type {actualToken.Type}.");
            }
            if (Regex != null && System.Text.RegularExpressions.Regex.Match(actualToken.Value<string>(), Regex).Value != actualToken.Value<string>())
            {
                differences.Add($"Expected value matching \'{Regex}\' (like: \'{expectedToken.Value<string>()}\') at {expectedToken.Path}, but was \'{actualToken.Value<string>()}\'.");
            }
            if (MatcherType == MatcherType.include && !actualToken.Value<string>().Contains(Value))
            {
                differences.Add($"Expected value at {expectedToken.Path} to include '{Value}', but was {actualToken.Value<string>()}.");
            }
            if (MatcherType == MatcherType.integer && actualToken.Type != JTokenType.Integer)
            {
                differences.Add($"Expected integer (like: \'{expectedToken.Value<int>()}\') at {expectedToken.Path}, but was {actualToken.ToString()}.");
            }
            if (MatcherType == MatcherType.@decimal && actualToken.Type != JTokenType.Float)
            {
                differences.Add($"Expected decimal (like: \'{expectedToken.Value<double>()}\') at {expectedToken.Path}, but was {actualToken.ToString()}.");
            }
            if (MatcherType == MatcherType.@null && actualToken.Type != JTokenType.Null)
            {
                differences.Add($"Expected null at {expectedToken.Path}, but was {actualToken}.");
            }

            if (expectedToken.Type == JTokenType.Array)
            {
                if (Min != null && actualToken.Children().Count() < Min)
                {
                    differences.Add($"Expected an array with at least {Min} item(s) at {expectedToken.Path}, but was {actualToken.Children().Count()} items(s).");
                }
                if (Max != null && actualToken.Children().Count() > Max)
                {
                    differences.Add($"Expected an array with at most {Max} item(s) at {expectedToken.Path}, but was {actualToken.Children().Count()} items(s).");
                }
            }

            return differences;
        }

        internal List<string> Match(object expectedValue, object actualValue)
        {
            return Match(JToken.FromObject(expectedValue), JToken.FromObject(actualValue));
        }
    }
}
