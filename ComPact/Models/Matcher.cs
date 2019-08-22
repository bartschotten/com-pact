﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models
{
    internal class Matcher
    {
        [JsonProperty("match")]
        [JsonConverter(typeof(StringEnumConverter))]
        internal MatcherType MatcherType { get; set; }
        [JsonProperty("min")]
        internal int? Min { get; set; }
        [JsonProperty("max")]
        internal int? Max { get; set; }
        [JsonProperty("regex")]
        internal string Regex { get; set; }

        internal List<string> Match(JToken expectedToken, JToken actualToken)
        {
            var differences = new List<string>();

            if (MatcherType == MatcherType.type && expectedToken.Type != actualToken.Type)
            {
                differences.Add($"Expected value of type {expectedToken.Type} (like: \'{expectedToken.ToString()}\') at {expectedToken.Path}, but was value of type {actualToken.Type}.");
            }
            if (Regex != null && System.Text.RegularExpressions.Regex.Match(actualToken.Value<string>(), Regex).Value != actualToken.Value<string>())
            {
                differences.Add($"Expected value matching \'{Regex}\' (like: \'{expectedToken.Value<string>()}\') at {expectedToken.Path}, but was \'{actualToken.Value<string>()}\'.");
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
