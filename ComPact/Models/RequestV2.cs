﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System;
using System.IO;
using System.Text;

namespace ComPact.Models
{
    public class RequestV2
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

        public RequestV2() { }

        public RequestV2(HttpRequest request)
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

            Query = request.QueryString.HasValue ? request.QueryString.Value.Substring(1) : string.Empty;

            var streamReader = new StreamReader(request.Body, Encoding.UTF8);
            var serializedBody = streamReader.ReadToEnd();
            Body = JsonConvert.DeserializeObject<dynamic>(serializedBody);
        }

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

        public bool Match(RequestV2 actualRequest)
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