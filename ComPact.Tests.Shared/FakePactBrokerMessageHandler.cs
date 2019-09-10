using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComPact.Tests.Shared
{
    public class FakePactBrokerMessageHandler: HttpMessageHandler
    {
        public Dictionary<string, string> SentRequestContents { get; private set; } = new Dictionary<string, string>();
        public HttpStatusCode StatusCodeToReturn { get; set; } = HttpStatusCode.OK;
        public object ObjectToReturn { get; set; } 

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseMessage = new HttpResponseMessage(StatusCodeToReturn);

            if (request.Method == HttpMethod.Get)
            {
                var jobjectToReturn = JObject.FromObject(ObjectToReturn);
                jobjectToReturn.Add("_links", new JObject(new JProperty("pb:publish-verification-results", new JObject(new JProperty("href", "url")))));
                responseMessage.Content = new StringContent(JsonConvert.SerializeObject(jobjectToReturn), Encoding.UTF8, "application/json");
            }
            else
            {
                SentRequestContents.Add(request.RequestUri.ToString(), request.Content.ReadAsStringAsync().Result);
            }

            return Task.FromResult(responseMessage);
        }
    }
}
