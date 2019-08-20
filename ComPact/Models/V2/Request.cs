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

        public RestRequest ToRestRequest()
        {
            var method = (RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), Method.ToString());
            var path = Path;
            if (!string.IsNullOrWhiteSpace(Query))
            {
                path += ("?" + Query);
            }
            var request = new RestRequest(path, method);
            foreach (var header in Headers)
            {
                request.AddHeader(header.Key, header.Value);
            }
            request.AddJsonBody(Body);

            return request;
        }

        public bool Match(Request actualRequest)
        {
            if (actualRequest == null)
            {
                throw new ArgumentNullException(nameof(actualRequest));
            }

            var methodsMatch = Method == actualRequest.Method;
            var pathsMatch = Path == actualRequest.Path;
            var headersMatch = Headers.Match(actualRequest.Headers);
            var queriesMatch = Query == actualRequest.Query;
            var bodiesMatch = Body == actualRequest.Body;

            return methodsMatch && pathsMatch && headersMatch && queriesMatch && bodiesMatch;
        }
    }
}