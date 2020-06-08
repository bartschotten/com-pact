using ComPact.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ComPact.Models.V3
{
    internal class Request
    {
        private const string ContentTypeHeader = "Content-Type";

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

        public Request(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!Enum.TryParse<Method>(request.Method, true, out var method))
            {
                throw new PactException($"Received method {request.Method} is not allowed in Pact contracts.");
            }
            Method = method;
            Path = request.Path.HasValue ? request.Path.Value : throw new PactException("Received path must have a value.");
            Headers = new Headers(request.Headers);
            Query = new Query(request.Query);

            using (var streamReader = new StreamReader(request.Body, Encoding.UTF8))
            {
                var serializedBody = streamReader.ReadToEnd();
                Body = JsonConvert.DeserializeObject<dynamic>(serializedBody);
            }
        }

        public HttpRequestMessage ToHttpRequestMessage()
        {
            var method = new HttpMethod(Method.ToString());
            var uri = Query.Any() ? $"{Path}?{Query.ToQueryString()}" : Path;
            var request = new HttpRequestMessage(method, uri);
             
            foreach (var header in GetHeadersExceptContentType())
            {
                request.Headers.Add(header.Key, header.Value);
            }
            if (Body != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(Body), Encoding.UTF8, GetContentTypeHeader("application/json"));
            }

            return request;
        }

        private IEnumerable<KeyValuePair<string, string>> GetHeadersExceptContentType()
        {
            return Headers.Where(h => !string.Equals(h.Key, ContentTypeHeader, StringComparison.OrdinalIgnoreCase));
        }

        private string GetContentTypeHeader(string defaultValue)
        {
            var header=  Headers.SingleOrDefault(h =>
                       string.Equals(h.Key, ContentTypeHeader, StringComparison.OrdinalIgnoreCase));
            return header.Value ?? defaultValue;
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
            var queriesMatch = Query.Match(actualRequest.Query);
            var bodiesMatch = MatchBody(actualRequest);

            return methodsMatch && pathsMatch && headersMatch && queriesMatch && bodiesMatch;
        }

        private dynamic MatchBody(Request actualRequest)
        {
            if (Body != null && actualRequest.Body != null)
            {
                return JObject.DeepEquals(JToken.FromObject(Body), JToken.FromObject(actualRequest.Body));
            }
            return Body == actualRequest.Body;
        }

        internal void SetEmptyValuesToNull()
        {
            Headers = Headers.Any() ? Headers : null;
            Query = Query.Any() ? Query : null;
        }
    }
}
