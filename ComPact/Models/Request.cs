﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace ComPact.Models
{
    public class Request
    {
        [JsonProperty("method")]
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

            Query = request.QueryString.HasValue ? request.QueryString.Value.Substring(1) : string.Empty;

            var streamReader = new StreamReader(request.Body, Encoding.UTF8);
            var serializedBody = streamReader.ReadToEnd();
            Body = JsonConvert.DeserializeObject<dynamic>(serializedBody);
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