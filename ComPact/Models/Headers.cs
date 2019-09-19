using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ComPact.Models
{
    public class Headers : Dictionary<string, string>
    {
        public Headers() { }

        internal Headers(IHeaderDictionary headers)
        {
            if (headers == null)
            {
                throw new System.ArgumentNullException(nameof(headers));
            }

            foreach (var header in headers)
            {
                Add(header.Key, string.Join(",", header.Value));
            }
        }

        internal Headers(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new System.ArgumentNullException(nameof(response));
            }

            response.Headers.ToList().ForEach(h => Add(h.Key, string.Join(",", h.Value)));
            response.Content?.Headers.ToList().ForEach(h => Add(h.Key, string.Join(",", h.Value)));
        }

        internal bool Match(Headers actualHeaders)
        {
            if (actualHeaders == null)
            {
                throw new System.ArgumentNullException(nameof(actualHeaders));
            }

            return this.All(h => actualHeaders.Any(a => h.Key == a.Key && h.Value == a.Value));
        }

        internal List<string> Match(Headers actualHeaders, MatchingRuleCollection matchingRules)
        {
            if (actualHeaders == null)
            {
                throw new System.ArgumentNullException(nameof(actualHeaders));
            }

            var differences = new List<string>();
            foreach(var expectedHeader in KeysToLowerCase(this))
            {
                if (KeysToLowerCase(actualHeaders).TryGetValue(expectedHeader.Key, out var actualHeaderValue))
                {
                    var expectedParts = SplitValueIntoParts(expectedHeader.Value);
                    var actualParts = SplitValueIntoParts(actualHeaderValue);

                    if (matchingRules?.Header != null && KeysToLowerCase(matchingRules.Header).TryGetValue(expectedHeader.Key, out var matchers))
                    {
                        differences.AddRange(matchers.Match(expectedHeader.Value, actualHeaderValue));
                    }
                    else if (!expectedParts.All(e => actualParts.Any(a => RemoveWhiteSpaceAfterCommas(a) == RemoveWhiteSpaceAfterCommas(e))))
                    {
                        differences.Add($"Expected {expectedHeader.Value} for {expectedHeader.Key}, but was {actualHeaderValue}");
                    }
                }
                else
                {
                    differences.Add($"Expected a header named {expectedHeader.Key}, but was not found.");
                }
            }

            return differences;
        }

        private Dictionary<string, T> KeysToLowerCase<T>(Dictionary<string, T> headers)
        {
            return headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);
        }

        private List<string> SplitValueIntoParts(string value)
        {
            return value.Split(";").Select(p => p.Trim()).ToList();
        }

        private string RemoveWhiteSpaceAfterCommas(string value)
        {
            return string.Join(',', value.Split(',').Select(x => x.Trim()));
        }
    }
}
