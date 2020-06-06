using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ComPact.JsonHelpers
{
    internal static class JTokenParser
    {
        internal static JToken Parse(dynamic body) => Parse<JsonTextReader>(body);
        internal static JToken ParseToLower(dynamic body) => Parse<LowerCaseJsonReader>(body);

        private static JToken Parse<T>(dynamic body) where T : JsonTextReader
        {
            // This is necessary because DateParseHandling.None has no effect when using JToken.Parse or JToken.FromObject directly.
            var jsonTextReader = new JsonTextReader(new StringReader(JsonConvert.SerializeObject(body)))
            {
                // We don't want datetime-like strings to be converted to datetime, otherwise regex matching doesn't work.
                DateParseHandling = DateParseHandling.None
            };
            return JToken.Load(jsonTextReader);
        }
    }

    internal class LowerCaseJsonReader : JsonTextReader
    {
        public LowerCaseJsonReader(TextReader reader) : base(reader) { }

        public override object Value
        {
            get => TokenType == JsonToken.PropertyName ? ((string)base.Value).ToLowerInvariant() : base.Value;
        }
    }
}
