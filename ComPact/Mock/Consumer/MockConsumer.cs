using System;
using RestSharp;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using ComPact.Models;
using ComPact.Models.V3;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task VerifyPact(string filePath)
        {
            var pactFile = ReadPactFile(filePath);

            Contract pact = DeserializePactFile(pactFile);

            var failedInteractions = new List<FailedInteraction>();

            if (pact.Interactions != null)
            {
                failedInteractions.AddRange(VerifyInteractions(pact.Interactions));
            }

            if (pact.Messages != null)
            {
                failedInteractions.AddRange(VerifyMessages(pact.Messages));
            }

            if (_config.PublishVerificationResults)
            {
                await PublishVerificationResults(pact, failedInteractions);
            }

            if (failedInteractions.Any())
            {
                throw new PactException(string.Join(Environment.NewLine, failedInteractions.Select(f => f.ToTestMessageString())));
            }
        }

        internal string ReadPactFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch
            {
                throw new PactException($"Could not read file at {filePath}");
            }
        }

        internal Contract DeserializePactFile(string pactFile)
        {
            try
            {
                var contractWithSomeVersion = JsonConvert.DeserializeObject<ContractWithSomeVersion>(pactFile);
                if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Three)
                {
                    return JsonConvert.DeserializeObject<Contract>(pactFile);
                }
                else if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Two)
                {
                    var pactV2 = JsonConvert.DeserializeObject<Models.V2.Contract>(pactFile);
                    return new Contract(pactV2);
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
            return null;
        }

        internal List<FailedInteraction> VerifyInteractions(List<Interaction> interactions)
        {
            var failedInteractions = new List<FailedInteraction>();

            if (_config.ProviderBaseUrl == null)
            {
                throw new PactException("Could not verify pacts. Please configure a ProviderBaseUrl.");
            }
            var client = new RestClient(_config.ProviderBaseUrl);
            foreach (var interaction in interactions)
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
                    failedInteractions.Add(new FailedInteraction { Description = interaction.Description, Differences = differences });
                }
            }

            return failedInteractions;
        }

        internal List<FailedInteraction> VerifyMessages(List<Message> messages)
        {
            var failedInteractions = new List<FailedInteraction>();

            foreach (var message in messages)
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
                    failedInteractions.Add(new FailedInteraction { Description = message.Description, Differences = differences });
                }
            }

            return failedInteractions;
        }

        internal async Task PublishVerificationResults(Contract pact, List<FailedInteraction> failedInteractions)
        {
            if (_config.PactBrokerClient?.BaseAddress == null)
            {
                throw new PactException("PactBrokerClient with at least a BaseAddress should be configured to be able to publish verification results.");
            }

            if (string.IsNullOrWhiteSpace(_config.ProviderVersion))
            {
                throw new PactException("ProviderVersion should be configured to be able to publish verification results.");
            }

            var verificationResults = new VerificationResults
            {
                ProviderName = pact.Provider.Name,
                ProviderApplicationVersion = _config.ProviderVersion,
                Success = !failedInteractions.Any(),
                VerificationDate = DateTime.Now.ToString("u"),
                FailedInteractions = failedInteractions
            };
            var content = new StringContent(JsonConvert.SerializeObject(verificationResults));

            var response = await _config.PactBrokerClient.PostAsync($"pacts/provider/{pact.Provider.Name}/consumer/{pact.Consumer.Name}/pact-version/{_config.ProviderVersion}/verification-results", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new PactException("Publishing verification results failed. Pact Broker returned " + response.StatusCode);
            }
        }
    }
}
