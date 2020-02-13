using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ComPact.Tests.Shared
{
    public class FakePactBrokerTagMessageHandler : HttpMessageHandler
    {
        public FakePactBrokerTagMessageHandler(HttpMethod httpMethod, string uriToMatch)
        {
            HttpMethodToMatch = httpMethod;
            UriToMatch = uriToMatch;
            CalledUrls = new List<string>();
        }

        public HttpStatusCode StatusCodeToReturn { get; set; } = HttpStatusCode.Created;
        public Exception ExceptionToThrow { get; set; }
        public HttpMethod HttpMethodToMatch { get; set; }
        public string UriToMatch { get; set; }
        public List<string> CalledUrls { get; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            if (request.Method == HttpMethodToMatch && request.RequestUri.AbsoluteUri.StartsWith(UriToMatch))
            {
                CalledUrls.Add(request.RequestUri.AbsoluteUri);
                var responseMessage = new HttpResponseMessage(StatusCodeToReturn);
                return Task.FromResult(responseMessage);
            }

            throw new ApplicationException("FakePactBrokerTagMessageHandler setup was not matched");
        }
    }
}
