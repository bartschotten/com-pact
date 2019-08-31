using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ComPact.Tests.Shared
{
    public class FakeHttpMessageHandler: HttpMessageHandler
    {
        public Dictionary<string, string> SentRequestContents { get; private set; } = new Dictionary<string, string>();
        public HttpStatusCode StatusCodeToReturn { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SentRequestContents.Add(request.RequestUri.ToString(), request.Content.ReadAsStringAsync().Result);
            return Task.FromResult(new HttpResponseMessage(StatusCodeToReturn));
        }
    }
}
