using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models.V3
{
    internal class Query : Dictionary<string, List<string>>
    {
        internal Query() { }

        internal Query(string queryString)
        {
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                var parameters = queryString.Split('&');
                foreach (var param in parameters)
                {
                    var splitParam = param.Split('=');
                    var valuesToAdd = splitParam[1].Split(',').ToList();
                    if (TryGetValue(splitParam[0], out var existingValues))
                    {
                        Remove(splitParam[0]);
                        valuesToAdd.AddRange(existingValues);
                    }
                    Add(splitParam[0], valuesToAdd);
                }
            }
        }

        internal Query(IQueryCollection queryCollection)
        {
            queryCollection.ToList().ForEach(q => Add(q.Key, q.Value.ToList()));
        }

        internal bool Match(Query actualQuery)
        {
            if (Count != actualQuery.Count)
            {
                return false;
            }
            foreach (var expectedParam in this)
            {
                var existsInActual = actualQuery.TryGetValue(expectedParam.Key, out var actualValues);
                if (!existsInActual)
                {
                    return false;
                }
                if (expectedParam.Value.Count != actualValues.Count)
                {
                    return false;
                }
                if (string.Join(",", expectedParam.Value) != string.Join(",", actualValues))
                {
                    return false;
                }
            }

            return true;
        }

        internal string ToQueryString()
        {
            var queryString = string.Join("&", this.Select(q => q.Key + "=" + string.Join(",", q.Value)));
            return queryString;
        }
    }
}
