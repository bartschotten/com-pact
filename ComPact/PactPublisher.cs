using ComPact.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ComPact
{
    public class PactPublisher
    {
        private readonly HttpClient _pactBrokerClient;
        private readonly string _consumerVersion;
        private readonly string _consumerTag;

        public PactPublisher(HttpClient pactBrokerClient, string consumerVersion, string consumerTag = null)
        {
            if (pactBrokerClient?.BaseAddress == null)
            {
                throw new PactException("A pactBrokerClient with at least a BaseAddress should be configured to be able to publish contracts.");
            }

            if (string.IsNullOrWhiteSpace(consumerVersion))
            {
                throw new PactException("ConsumerVersion should be configured to be able to publish contracts.");
            }

            _pactBrokerClient = pactBrokerClient ?? throw new ArgumentNullException(nameof(pactBrokerClient));

            _consumerVersion = consumerVersion ?? throw new ArgumentNullException(nameof(consumerVersion));
            _consumerTag = consumerTag;
        }

        internal async Task Publish(IContract pact)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };

            var content = new StringContent(JsonConvert.SerializeObject(pact, settings), Encoding.UTF8, "application/json");

            var response = await _pactBrokerClient.PutAsync($"pacts/provider/{pact.Provider.Name}/consumer/{pact.Consumer.Name}/version/{_consumerVersion}", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new PactException("Publishing contract failed. Pact Broker returned " + response.StatusCode);
            }
        }
    }
}
