﻿using ComPact.JsonHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ComPact.Models
{
    internal static class BodyMatcher
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
                differences.Add("Expected body or contents to be present, but was not.");
                return differences;
            }

            JToken expectedRootToken = JTokenParser.Parse(expectedBody);
            JToken actualRootToken = JTokenParser.Parse(actualBody);

            foreach (var token in expectedRootToken.ThisTokenAndAllItsDescendants())
            {
                differences.AddRange(ProcessToken(token, matchingRules, actualRootToken));
            }

            differences.AddRange(VerifyAdditionalActualTokensAgainstMatchingRules(expectedRootToken, actualRootToken, matchingRules));

            return differences;
        }

        private static List<string> VerifyAdditionalActualTokensAgainstMatchingRules(JToken expectedRootToken, JToken actualRootToken, MatchingRuleCollection matchingRules)
        {
            var differences = new List<string>();

            if (matchingRules?.Body == null || !matchingRules.Body.Any())
            {
                return differences;
            }

            var pathsInExpected = expectedRootToken.ThisTokenAndAllItsDescendants().Select(t => t.Path);
            var additionalActualTokens = actualRootToken.ThisTokenAndAllItsDescendants().Where(t => !pathsInExpected.Contains(t.Path) && t.Type != JTokenType.Property);
            foreach (var token in additionalActualTokens)
            {
                // Find each MatchingRule that applies directly to the additional token,
                // excluding those that apply to all members in an object (*), because the specification isn't clear how to handle those
                foreach (var jsonPath in matchingRules.Body.Select(b => b.Key).Where(p => !p.EndsWith(".*")))
                {
                    if (actualRootToken.SelectTokens(jsonPath).Any(t => t.Path == token.Path))
                    {
                        var expectedSibling = expectedRootToken.SelectTokens(jsonPath).FirstOrDefault();
                        if (expectedSibling != null)
                        {
                            differences.AddRange(matchingRules.Body[jsonPath].Match(expectedSibling, token));
                        }
                    }
                }
            }

            return differences;
        }

        private static List<string> ProcessToken(JToken expectedToken, MatchingRuleCollection matchingRules, JToken actualRootToken)
        {
            switch (expectedToken.Type)
            {
                case JTokenType.String:
                case JTokenType.Null:
                    return CompareExpectedTokenWithActual<string>(expectedToken, matchingRules, actualRootToken);
                case JTokenType.Integer:
                    return CompareExpectedTokenWithActual<long>(expectedToken, matchingRules, actualRootToken);
                case JTokenType.Float:
                    return CompareExpectedTokenWithActual<double>(expectedToken, matchingRules, actualRootToken);
                case JTokenType.Boolean:
                    return CompareExpectedTokenWithActual<bool>(expectedToken, matchingRules, actualRootToken);
                case JTokenType.Array:
                    return CompareExpectedArrayWithActual(expectedToken, matchingRules, actualRootToken);
                default:
                    return new List<string>();
            }
        }

        private static List<string> CompareExpectedTokenWithActual<T>(JToken expectedToken, MatchingRuleCollection matchingRules, JToken actualRootToken) where T : IEquatable<T>
        {
            var expectedValue = expectedToken.Value<T>();
            var actualToken = actualRootToken.SelectToken(expectedToken.Path);
            if (actualToken == null)
            {
                var actualRootTokenWithLowerCasePropertyNames = JTokenParser.ParseToLower(actualRootToken);
                if (actualRootTokenWithLowerCasePropertyNames.SelectToken(expectedToken.Path.ToLowerInvariant()) != null)
                {
                    return new List<string> { $"A property with a name like \'{expectedToken.Path}\' was present in the actual response, but the case did not match. Note that Pact is case sensitive." };
                }
                return new List<string> { $"Property \'{expectedToken.Path}\' was not present in the actual response." };
            }

            if (!expectedToken.IsSameJsonTypeAs(actualToken))
            {
                return new List<string> { $"Property \'{expectedToken.Path}\' has a different type in the actual response. Expected value: {expectedToken}, actual value: {actualToken}" };
            }

            var actualValue = actualToken.Value<T>();
            if (expectedToken.Type != JTokenType.Null && actualValue == null)
            {
                return new List<string> { $"Expected \'{expectedValue}\' at \'{expectedToken.Path}\', but had no value." };
            }
            if (matchingRules != null && matchingRules.TryGetApplicableMatcherListForToken(expectedToken, out var matcherList))
            {
                return matcherList.Match(expectedToken, actualToken);
            }
            else if (!actualValue.Equals(expectedValue))
            {
                return new List<string> { $"Expected \'{expectedValue}\' at \'{expectedToken.Path}\', but was \'{actualValue}\'." };
            }

            return new List<string>();
        }

        private static List<string> CompareExpectedArrayWithActual(JToken expectedToken, MatchingRuleCollection matchingRules, JToken actualRootToken)
        {
            var actualToken = actualRootToken.SelectToken(expectedToken.Path);
            if (actualToken == null)
            {
                return new List<string> { $"Array \'{expectedToken.Path}\' was not present in the actual response." };
            }
            if (matchingRules != null && matchingRules.TryGetApplicableMatcherListForToken(expectedToken, out var matcherList))
            {
                return matcherList.Match(expectedToken, actualToken);
            }
            else if (actualToken.Children().Count() != expectedToken.Children().Count())
            {
                return new List<string> { $"Expected array at \'{expectedToken.Path}\' to have {expectedToken.Children().Count()} items, but was {actualToken.Children().Count()}." };
            }

            return new List<string>();
        }
    }
}
