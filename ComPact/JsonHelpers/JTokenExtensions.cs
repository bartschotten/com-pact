using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.JsonHelpers
{
    internal static class JTokenExtensions
    {
        internal static IEnumerable<JToken> ThisTokenAndAllItsDescendants(this JToken token)
        {
            var tokenAndItsChildren = new List<JToken> { token };
            tokenAndItsChildren.AddRange(token.Children().Select(c => c.ThisTokenAndAllItsDescendants()).SelectMany(d => d));
            return tokenAndItsChildren;
        }

        internal static bool IsSameJsonTypeAs(this JToken token, JToken otherToken)
        {
             return token.Type == otherToken.Type ||
                    (token.Type == JTokenType.Float && otherToken.Type == JTokenType.Integer) ||
                    (token.Type == JTokenType.Integer && otherToken.Type == JTokenType.Float);
        }
    }
}
