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
using System.Text;
using Newtonsoft.Json.Linq;

namespace ComPact.Mock.Consumer
{
    public class MockConsumer
    {
        private readonly MockConsumerConfig _config;
        private string _publishVerificationResultsPath;

        /// <summary>
        /// Set up a mock consumer that will call your code based on the defined interactions or messages in a supplied Pact contract.
        /// </summary>
        /// <param name="config"></param>
        public MockConsumer(MockConsumerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Will be interpreted as Pact Broker path when a Pact Broker Client is configured and as a file path otherwise.</param>
        /// <returns></returns>
        public async Task VerifyPactAsync(string path)
        {
            var pactContent = await RetrievePactContent(path);

            Contract pact = DeserializePactContent(pactContent);

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
                await PublishVerificationResultsAsync(pact, failedInteractions);
            }

            if (failedInteractions.Any())
            {
                throw new PactException(string.Join(Environment.NewLine, failedInteractions.Select(f => f.ToTestMessageString())));
            }
        }

        internal async Task<string> RetrievePactContent(string path)
        {
            if (_config.PactBrokerClient != null)
            {
                return await GetPactFromBroker(path);
            }
            else
            {
                return ReadPactFile(path);
            }
        }

        internal async Task<string> GetPactFromBroker(string path)
        {
            if (_config.PactBrokerClient.BaseAddress == null)
            {
                throw new PactException("A PactBrokerClient with at least a BaseAddress should be configured to be able to retrieve contracts.");
            }

            HttpResponseMessage response;
            try
            {
                response = await _config.PactBrokerClient.GetAsync(path);
            }
            catch (Exception e)
            {
                throw new PactException($"Pact cannot be retrieved using the provided Pact Broker Client: {e.Message}");
            }
            if (!response.IsSuccessStatusCode)
            {
                throw new PactException("Getting pact from Pact Broker failed. Pact Broker returned " + response.StatusCode);
            }

            var stringContent = await response.Content.ReadAsStringAsync();
            var pactJObject = JObject.Parse(stringContent);
            _publishVerificationResultsPath = pactJObject.SelectToken("_links.pb:publish-verification-results.href").Value<string>();

            return stringContent;
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

        internal Contract DeserializePactContent(string pactContent)
        {
            try
            {
                var contractWithSomeVersion = JsonConvert.DeserializeObject<ContractWithSomeVersion>(pactContent);
                if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Three)
                {
                    return JsonConvert.DeserializeObject<Contract>(pactContent);
                }
                else if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Two)
                {
                    var pactV2 = JsonConvert.DeserializeObject<Models.V2.Contract>(pactContent);
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
                object providedMessage = null;
                try
                {
                    providedMessage = _config.MessageProducer.Invoke(message.ProviderStates, message.Description);
                }
                catch
                {
                    throw new PactException("Exception occured while invoking MessageProducer.");
                }

                if (providedMessage is string messageString)
                {
                    providedMessage = JsonConvert.DeserializeObject<dynamic>(messageString);
                }
                else
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

        internal async Task PublishVerificationResultsAsync(Contract pact, List<FailedInteraction> failedInteractions)
        {
            if (string.IsNullOrWhiteSpace(_config.ProviderVersion))
            {
                throw new PactException("ProviderVersion should be configured to be able to publish verification results.");
            }

            var verificationResults = new VerificationResults
            {
                ProviderName = pact.Provider.Name,
                ProviderApplicationVersion = _config.ProviderVersion,
                Success = !failedInteractions.Any(),
                VerificationDate = DateTime.UtcNow.ToString("u"),
                FailedInteractions = failedInteractions
            };
            var content = new StringContent(JsonConvert.SerializeObject(verificationResults), Encoding.UTF8, "application/json");

            var response = await _config.PactBrokerClient.PostAsync(_publishVerificationResultsPath, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new PactException("Publishing verification results failed. Pact Broker returned " + response.StatusCode);
            }
        }
    }
}
