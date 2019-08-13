using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ComPact.Models
{
    internal class Matcher
    {
        [JsonProperty("match")]
        internal string MatcherType { get; set; }
        [JsonProperty("min")]
        internal uint? Min { get; set; }
        [JsonProperty("max")]
        internal uint? Max { get; set; }
        [JsonProperty("regex")]
        internal string Regex { get; set; }

        internal string Match<T>(JToken expectedToken, JToken actualToken)
        {
            if (MatcherType == "type" && expectedToken.Type != actualToken.Type)
            {
                return $"Expected value of type {expectedToken.Type} (like: {expectedToken.Value<T>()}) at {expectedToken.Path}, but was value of type {actualToken.Type}.";
            }
            if (Regex != null && !System.Text.RegularExpressions.Regex.IsMatch(actualToken.Value<string>(), Regex))
            {
                return $"Expected value matching {Regex} (like: {expectedToken.Value<T>()}) at {expectedToken.Path}, but was {actualToken.Value<T>()}.";
            }
            if (Min != null && actualToken.Children().Count() < Min)
            {
                return $"Expected an array with at least {Min} item(s) at {expectedToken.Path}, but was {actualToken.Children().Count()} items(s).";
            }
            if (Max != null && actualToken.Children().Count() > Max)
            {
                return $"Expected an array with at most {Max} item(s) at {expectedToken.Path}, but was {actualToken.Children().Count()} items(s).";
            }

            return null;
        }
    }
}
