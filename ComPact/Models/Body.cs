using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models
{
    internal static class Body
    {
        internal static List<string> Match(dynamic expectedBody, dynamic actualBody, MatchingRuleCollection matchingRules)
        {
            var differences = new List<string>();

            if (expectedBody == null)
            {
                return differences;
            }

            if (actualBody == null)
            {
                differences.Add("Expected body to be present, but was not.");
                return differences;
            }

            var expectedToken = JToken.FromObject(expectedBody);
            var actualToken = JToken.FromObject(actualBody);

            FillOutExpectedTokensToMatchAdditionalActualTokens(expectedToken, actualToken, matchingRules);
            ProcessTokenAndItsChildren(expectedToken, matchingRules, actualToken, differences);

            return differences;
        }

        private static void FillOutExpectedTokensToMatchAdditionalActualTokens(JToken expectedRootToken, JToken actualRootToken, MatchingRuleCollection matchingRules)
        {
            if (matchingRules?.Body == null)
            {
                return;
            }
            foreach (var path in matchingRules.Body.Select(m => m.Key))
            {
                var matchingActualTokens = actualRootToken.SelectTokens(path).ToList();
                foreach (var actualToken in matchingActualTokens)
                {
                    var expectedToken = expectedRootToken.SelectToken(actualToken.Path);
                    if (expectedToken == null)
                    {
                        var expectedTokenParent = expectedRootToken.SelectToken(actualToken.Parent.Path);
                        expectedTokenParent?.First.AddAfterSelf(expectedTokenParent.First);
                    }
                }
            }
        }

        private static void ProcessTokenAndItsChildren(JToken expectedToken, MatchingRuleCollection matchingRules, JToken actualRootToken, List<string> differences)
        {
            switch (expectedToken.Type)
            {
                case JTokenType.String:
                case JTokenType.Null:
                    differences.AddRange(CompareExpectedTokenWithActual<string>(expectedToken, matchingRules, actualRootToken));
                    break;
                case JTokenType.Integer:
                    differences.AddRange(CompareExpectedTokenWithActual<long>(expectedToken, matchingRules, actualRootToken));
                    break;
                case JTokenType.Float:
                    differences.AddRange(CompareExpectedTokenWithActual<double>(expectedToken, matchingRules, actualRootToken));
                    break;
                case JTokenType.Boolean:
                    differences.AddRange(CompareExpectedTokenWithActual<bool>(expectedToken, matchingRules, actualRootToken));
                    break;
                case JTokenType.Array:
                    differences.AddRange(CompareExpectedArrayWithActual(expectedToken, actualRootToken));
                    break;
            }

            foreach (var child in expectedToken.Children())
            {
                ProcessTokenAndItsChildren(child, matchingRules, actualRootToken, differences);
            }
        }

        private static List<string> CompareExpectedTokenWithActual<T>(JToken expectedToken, MatchingRuleCollection matchingRules, JToken actualRootToken) where T : IEquatable<T>
        {
            var expectedValue = expectedToken.Value<T>();
            var actualToken = actualRootToken.SelectToken(expectedToken.Path);
            if (actualToken == null)
            {
                return new List<string> { $"Property \'{expectedToken.Path}\' was not present in the actual response." };
            }
            var actualValue = actualToken.Value<T>();
            if (actualValue == null)
            {
                return new List<string> { $"Expected \'{expectedValue}\' at \'{expectedToken.Path}\', but had no value." };
            }
            if (matchingRules != null && matchingRules.TryGetApplicableMatcherListForToken(expectedToken, out var matcherList))
            {
                return matcherList.Match(expectedToken, actualToken);
            }
            else if (expectedToken.Type != actualToken.Type)
            {
                return new List<string> { $"Expected value of type {expectedToken.Type} (like: \'{expectedToken.ToString()}\') at \'{expectedToken.Path}\', but was value of type {actualToken.Type}." };
            }
            else if (!actualValue.Equals(expectedValue))
            {
                return new List<string> { $"Expected \'{expectedValue}\' at \'{expectedToken.Path}\', but was \'{actualValue}\'." };
            }

            return new List<string>();
        }

        private static List<string> CompareExpectedArrayWithActual(JToken expectedToken, JToken actualRootToken)
        {
            var actualToken = actualRootToken.SelectToken(expectedToken.Path);
            if (actualToken == null)
            {
                return new List<string> { $"Array \'{expectedToken.Path}\' was not present in the actual response." };
            }
            else if (actualToken.Children().Count() != expectedToken.Children().Count())
            {
                return new List<string> { $"Expected array at \'{expectedToken.Path}\' to have {expectedToken.Children().Count()} items, but was {actualToken.Children().Count()}." };
            }

            return new List<string>();
        }
    }
}
