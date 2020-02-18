using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ComPact.Tests.Shared
{
    public class FakePactBrokerMessageHandler : HttpMessageHandler
    {
        public FakePactBrokerMessageHandler()
        {
            Configurations = new List<Configuration>();
            Statuses = new List<Status>();
        }

        private List<Configuration> Configurations { get; }
        private List<Status> Statuses { get; }
        

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var configuration = Configurations.SingleOrDefault(c =>
                c.HttpMethod == request.Method && request.RequestUri.AbsoluteUri.StartsWith(c.UriToMatch));
            var status = Statuses.SingleOrDefault(s =>
                s.HttpMethod == request.Method && request.RequestUri.AbsoluteUri.StartsWith(s.UriToMatch));

            if (configuration != null && status != null)
            {
                if (configuration.ExceptionToThrow != null)
                {
                    throw configuration.ExceptionToThrow;
                }

                var responseMessage = new HttpResponseMessage(configuration.HttpStatusCode);

                status.CalledUrls.Add(request.RequestUri.AbsoluteUri);
                if (request.Method == HttpMethod.Get)
                {
                    var returnObject = JObject.FromObject(configuration.ObjectToReturn);
                    returnObject.Add("_links", new JObject(new JProperty("pb:publish-verification-results", new JObject(new JProperty("href", "publish/verification/results/path")))));
                    responseMessage.Content = new StringContent(JsonConvert.SerializeObject(returnObject), Encoding.UTF8, "application/json");
                }
                else
                {
                    status.SentRequestContents.Add(request.RequestUri.ToString(), request.Content.ReadAsStringAsync().Result);
                }

                return Task.FromResult(responseMessage);
            }

            throw new ApplicationException("FakePactBrokerMessageHandler setup was not matched");
        }

        public Configuration Configure(HttpMethod httpMethod, string uriToMatch)
        {
            var configuration = new Configuration(httpMethod, uriToMatch);
            Configurations.Add(configuration);
            Statuses.Add(new Status(httpMethod, uriToMatch));
            return configuration;
        }

        public Status GetStatus(HttpMethod method, string uriToMatch)
        {
            return Statuses.SingleOrDefault(s => s.HttpMethod == method && string.Equals(s.UriToMatch, uriToMatch, StringComparison.OrdinalIgnoreCase));
        }

        public class Configuration
        {
            internal Configuration(HttpMethod method, string uriToMatch)
            {
                HttpMethod = method;
                UriToMatch = uriToMatch;
            }

            public Configuration RespondsWith(HttpStatusCode statusCode)
            {
                HttpStatusCode = statusCode;
                return this;
            }

            public Configuration ThowsException(Exception exception)
            {
                ExceptionToThrow = exception;
                return this;
            }

            public Configuration Returns(object objectToReturn)
            {
                ObjectToReturn = objectToReturn;
                return this;
            }

            internal HttpMethod HttpMethod { get; }
            internal string UriToMatch { get; }
            internal HttpStatusCode HttpStatusCode { get; private set; }
            internal Exception ExceptionToThrow { get; private set; }
            internal object ObjectToReturn { get; private set; }
        }

        public class Status
        {
            internal Status(HttpMethod method, string uriToMatch)
            {
                HttpMethod = method;
                UriToMatch = uriToMatch;
                SentRequestContents = new Dictionary<string, string>();
                CalledUrls = new List<string>();
            }

            public HttpMethod HttpMethod { get; }
            public string UriToMatch { get; }
            public Dictionary<string, string> SentRequestContents { get; set; }
            public List<string> CalledUrls { get; }
        }
    }
}
