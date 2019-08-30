using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ComPact.Tests.Shared
{
    public class FakeHttpMessageHandler: HttpMessageHandler
    {
        public string SentRequestContent { get; private set; }
        public HttpStatusCode StatusCodeToReturn { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SentRequestContent = request.Content.ReadAsStringAsync().Result;
            return Task.FromResult(new HttpResponseMessage(StatusCodeToReturn));
        }
    }
}
