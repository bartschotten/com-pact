using ComPact.Exceptions;
using ComPact.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ComPact.Builders
{
    public class PactPublisher
    {
        private readonly HttpClient _pactBrokerClient;
        private readonly string _consumerVersion;
        private readonly string _consumerTag;

        /// <summary>
        /// Publishes generated contracts to your Pact Broker.
        /// </summary>
        /// <param name="pactBrokerClient">Client that can be used to connect to your Pact Broker. Should be set up with the correct base URL and if needed any necessary headers.</param>
        /// <param name="consumerVersion">The version of your consumer application.</param>
        /// <param name="consumerTag">An optional tag to tag your consumer version with.</param>
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

        internal async Task PublishAsync(IContract pact)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };

            var content = new StringContent(JsonConvert.SerializeObject(pact, settings), Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            try
            {
                response = await _pactBrokerClient.PutAsync($"pacts/provider/{pact.Provider.Name}/consumer/{pact.Consumer.Name}/version/{_consumerVersion}", content);
            }
            catch (Exception e)
            {
                throw new PactException($"Pact cannot be published using the provided Pact Broker Client: {e.Message}");
            }
            if (!response.IsSuccessStatusCode)
            {
                throw new PactException("Publishing contract failed. Pact Broker returned " + response.StatusCode);
            }

            if (_consumerTag != null)
            {
                var tagResponse = await _pactBrokerClient.PutAsync($"pacticipants/{pact.Consumer.Name}/versions/{_consumerVersion}/tags/{_consumerTag}", new StringContent(string.Empty, Encoding.UTF8, "application/json"));
                if (!tagResponse.IsSuccessStatusCode)
                {
                    throw new PactException("Tagging consumer version failed. Pact Broker returned " + response.StatusCode);
                }
            }
        }
    }
}
