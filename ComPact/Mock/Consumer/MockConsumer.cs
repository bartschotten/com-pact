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
        private readonly MockConsumerConfig _config;

        /// <summary>
        /// Set up a mock consumer that will call your code based on the defined interactions or messages in a supplied Pact contract.
        /// </summary>
        /// <param name="config"></param>
        public MockConsumer(MockConsumerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
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
                if (_config.ProviderBaseUrl == null)
                {
                    throw new PactException("Could not verify pacts. Please configure a ProviderBaseUrl.");
                }
                var client = new RestClient(_config.ProviderBaseUrl);
                foreach (var interaction in pact.Interactions)
                {
                    if (interaction.ProviderStates != null)
                    {
                        var providerStatesRequest = new RestRequest("provider-states", RestSharp.Method.POST).AddJsonBody(interaction.ProviderStates);
                        var providerStateResponse = client.Execute(providerStatesRequest);
                        if (!providerStateResponse.IsSuccessful)
                        {
                            throw new PactException($"Could not set providerState '{interaction.ProviderStates}'. Got a {providerStateResponse.StatusCode} response.");
                        }
                    }

                    var restRequest = interaction.Request.ToRestRequest();
                    var actualResponse = client.Execute(restRequest);
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
                        _config.MessageProviderStateHandler.Invoke(message.ProviderStates);
                    }
                    catch
                    {
                        throw new PactException("Exception occured while invoking MessageProviderStateHandler.");
                    }

                    object providedMessage = null;
                    try
                    {
                        providedMessage = _config.MessageProducer.Invoke();
                    }
                    catch
                    {
                        throw new PactException("Exception occured while invoking MessageProducer.");
                    }

                    if (!(providedMessage is string))
                    {
                        providedMessage = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(providedMessage));
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
