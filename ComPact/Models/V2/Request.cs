using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System;

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
        public string Query { get; set; } = string.Empty;
        [JsonProperty("body")]
        public dynamic Body { get; set; }

        public Request() { }
    }
}