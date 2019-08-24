using System;
using RestSharp;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using ComPact.Models;
using ComPact.Models.V3;

namespace ComPact.Mock.Consumer
{
    public class MockConsumer
    {
        private readonly RestClient _client;
        private readonly Action<ProviderState> _messageProviderStateHandler;
        private readonly Func<object> _messageProducer;

        /// <summary>
        /// Set up a mock consumer that will call your code based on the defined interactions in a provider Pact contract.
        /// </summary>
        /// <param name="baseUrl">The baseUrl where the mock consumer will call your provider.</param>
        public MockConsumer(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException("message", nameof(baseUrl));
            }

            _client = new RestClient(baseUrl);
        }

        /// <summary>
        /// Set up a mock consumer that will call your code based on the defined interactions and messages in a provider Pact contract.
        /// </summary>
        /// <param name="baseUrl">The baseUrl where the mock consumer will call your provider for HTTP interactions.</param>
        /// <param name="messageProviderStateHandler">An action that will be invoked for every providerState of a message interaction.</param>
        /// <param name="messageProducer">A function that will be called to retrieve the actual message you produce.</param>
        public MockConsumer(string baseUrl, Action<ProviderState> messageProviderStateHandler, Func<object> messageProducer)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException("message", nameof(baseUrl));
            }

            _client = new RestClient(baseUrl);
            _messageProviderStateHandler = messageProviderStateHandler ?? throw new ArgumentNullException(nameof(messageProviderStateHandler));
            _messageProducer = messageProducer ?? throw new ArgumentNullException(nameof(messageProducer));
        }

        /// <summary>
        /// Set up a mock consumer that will call your code based on the defined messages in a provider Pact contract.
        /// </summary>
        /// <param name="messageProviderStateHandler">An action that will be invoked for every providerState of a message interaction.</param>
        /// <param name="messageProducer">A function that will be called to retrieve the actual message you produce.</param>
        public MockConsumer(Action<ProviderState> messageProviderStateHandler, Func<object> messageProducer)
        {
            _messageProviderStateHandler = messageProviderStateHandler ?? throw new ArgumentNullException(nameof(messageProviderStateHandler));
            _messageProducer = messageProducer ?? throw new ArgumentNullException(nameof(messageProducer));
        }

        public void VerifyPact(string filePath)
        {
            string pactFile;

            try
            {
                pactFile = File.ReadAllText(filePath);
            }
            catch
            {
                throw new PactException($"Could not read file at {filePath}");
            }

            Contract pact = null;

            try
            {
                var contractWithSomeVersion = JsonConvert.DeserializeObject<ContractWithSomeVersion>(pactFile);
                if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Three)
                {
                    pact = JsonConvert.DeserializeObject<Contract>(pactFile);
                }
                else if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Two)
                {
                    var pactV2 = JsonConvert.DeserializeObject<Models.V2.Contract>(pactFile);
                    pact = new Contract(pactV2);
                }
                else if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Unsupported)
                {
                    throw new PactException("Pact specification version is not supported.");
                }
            }
            catch
            {
                throw new PactException("File was not recognized as a valid Pact contract.");
            }

            if (pact.Interactions != null)
            {
                foreach (var interaction in pact.Interactions)
                {
                    if (interaction.ProviderState != null)
                    {
                        var providerStatesRequest = new RestRequest("provider-states", RestSharp.Method.POST).AddJsonBody(interaction.ProviderState);
                        var providerStateResponse = _client.Execute(providerStatesRequest);
                        if (!providerStateResponse.IsSuccessful)
                        {
                            throw new PactException($"Could not set providerState '{interaction.ProviderState}'. Got a {providerStateResponse.StatusCode} response.");
                        }
                    }

                    var restRequest = interaction.Request.ToRestRequest();
                    var actualResponse = _client.Execute(restRequest);
                    var differences = interaction.Response.Match(new Response(actualResponse));
                    if (differences.Any())
                    {
                        throw new PactException(string.Join(Environment.NewLine, differences));
                    }
                }
            }

            if (pact.Messages != null)
            {
                foreach (var message in pact.Messages)
                {
                    try
                    {
                        _messageProviderStateHandler.Invoke(message.ProviderState);
                    }
                    catch
                    {
                        throw new PactException("Exception occured while invoking messageProviderStateHandler.");
                    }

                    object providedMessage = null;
                    try
                    {
                        providedMessage = _messageProducer.Invoke();
                    }
                    catch
                    {
                        throw new PactException("Exception occured while invoking messageProducer.");
                    }

                    var differences = message.Match(providedMessage);
                    if (differences.Any())
                    {
                        throw new PactException(string.Join(Environment.NewLine, differences));
                    }
                }
            }
        }
    }
}
