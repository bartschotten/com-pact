using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace ComPact.Models.V2
{
    public class Request
    {
        [JsonProperty("method")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Method Method { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; } = "/";
        [JsonProperty("headers")]
        public Headers Headers { get; set; } = new Headers();
        [JsonProperty("query")]
        public string Query { get; set; }
        [JsonProperty("body")]
        public dynamic Body { get; set; }

        public Request() { }

        internal void SetEmptyValuesToNull()
        {
            Headers = Headers.Any() ? Headers : null;
            Query = string.IsNullOrWhiteSpace(Query) ? null : Query;
        }
    }
}