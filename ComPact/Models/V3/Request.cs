using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System;

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

        public Request() { }
        public Request(V2.Request request)
        {
            Method = request?.Method ?? throw new System.ArgumentNullException(nameof(request));
            Path = request.Path;
            Headers = request.Headers;
            Query = new Query(request.Query);
            Body = request.Body;
        }

        public RestRequest ToRestRequest()
        {
            var method = (RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), Method.ToString());
            var path = Path;
            if (Query != null)
            {
                path += ("?" + Query.ToString());
            }
            var request = new RestRequest(path, method);
            foreach (var header in Headers)
            {
                request.AddHeader(header.Key, header.Value);
            }
            request.AddJsonBody(Body);

            return request;
        }
    }
}
