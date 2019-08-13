using Microsoft.AspNetCore.Http;
using RestSharp;
using System.Collections.Generic;
using System.Linq;

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

        internal Headers(IList<Parameter> headers)
        {
            if (headers == null)
            {
                throw new System.ArgumentNullException(nameof(headers));
            }

            foreach (var header in headers)
            {
                if (header.Value is string stringValue)
                {
                    Add(header.Name, stringValue);
                }
            }
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
            foreach(var expectedHeader in this)
            {
                if (actualHeaders.TryGetValue(expectedHeader.Key, out var actualHeader))
                {
                    var expectedParts = expectedHeader.Value.ToLower().Split(";").Select(p => p.Trim()).ToList();
                    var actualParts = actualHeader.ToLower().Split(";").Select(p => p.Trim()).ToList();
                    if (!expectedParts.All(e => actualParts.Any(a => a.Equals(a == e))))
                    {
                        differences.Add($"Expected {expectedHeader.Value} for {expectedHeader.Key}, but was {actualHeader}");
                    }

                }
                else
                {
                    differences.Add($"Expected a header named {expectedHeader.Key}, but was not found.");
                }
            }

            return differences;
        }
    }
}
