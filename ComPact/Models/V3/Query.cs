using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models.V3
{
    internal class Query : Dictionary<string, string[]>
    {
        internal Query() { }

        internal Query(string queryString)
        {
            var parameters = queryString.Split("&");
            foreach (var param in parameters)
            {
                var splitParam = param.Split("=");
                if (TryGetValue(splitParam[0], out var existingValues))
                {
                    Remove(splitParam[0]);
                    splitParam[1] += existingValues;
                }
                Add(splitParam[0], splitParam[1].Split(","));
            }
        }

        internal string ToQueryString()
        {
            var queryString = string.Join("&", this.Select(q => q.Key + "=" + string.Join(",", q.Value)));
            return queryString;
        }
    }
}
