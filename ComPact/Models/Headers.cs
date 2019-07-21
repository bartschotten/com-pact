using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models
{
    public class Headers : Dictionary<string, string>
    {
        public Headers() { }

        public Headers(IHeaderDictionary headers)
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

        public bool Match(Headers actualHeaders)
        {
            if (actualHeaders == null)
            {
                throw new System.ArgumentNullException(nameof(actualHeaders));
            }

            return this.All(h => actualHeaders.Any(a => h.Key == a.Key && h.Value == a.Value));
        }
    }
}
