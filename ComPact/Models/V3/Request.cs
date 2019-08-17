using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ComPact.Models.V3
{
    internal class Request
    {
        [JsonProperty("method")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Method Method { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; } = "/";
        [JsonProperty("headers")]
        public Headers Headers { get; set; } = new Headers();
        [JsonProperty("query")]
        public Query Query { get; set; } = new Query();
        [JsonProperty("body")]
        public dynamic Body { get; set; }
    }
}
