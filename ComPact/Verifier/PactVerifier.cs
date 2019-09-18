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
using ComPact.Exceptions;

namespace ComPact.Verifier
{
    public class PactVerifier
    {
        private readonly PactVerifierConfig _config;
        private string _publishVerificationResultsPath;

        /// <summary>
        /// Set up a mock consumer that will call your code based on the defined interactions or messages in a supplied Pact contract.
        /// </summary>
        /// <param name="config"></param>
        public PactVerifier(PactVerifierConfig config)
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

            var tests = new List<Test>();

            if (pact.Interactions != null)
            {
                tests.AddRange(VerifyInteractions(pact.Interactions));
            }

            if (pact.Messages != null)
            {
                tests.AddRange(VerifyMessages(pact.Messages));
            }

            if (_config.PublishVerificationResults)
            {
                await PublishVerificationResultsAsync(pact, tests);
            }

            if (tests.Any(t => t.Status == "failed"))
            {
                throw new PactVerificationException(string.Join(Environment.NewLine, tests.Select(f => f.ToTestMessageString())));
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
            catch (Exception e)
            {
                throw new PactException($"File was not recognized as a valid Pact contract: {e.Message}");
            }
            return null;
        }

        internal List<Test> VerifyInteractions(List<Interaction> interactions)
        {
            var tests = new List<Test>();

            if (_config.ProviderBaseUrl == null)
            {
                throw new PactException("Could not verify pacts. Please configure a ProviderBaseUrl.");
            }
            var client = new RestClient(_config.ProviderBaseUrl);
            foreach (var interaction in interactions)
            {
                var test = new Test { Description = interaction.Description };
                var verificationMessages = InvokeProviderStateHandler(interaction.ProviderStates);
                if (verificationMessages.Any())
                {
                    test.Issues = verificationMessages;
                }
                else
                {
                    var restRequest = interaction.Request.ToRestRequest();
                    var actualResponse = client.Execute(restRequest);
                    var differences = interaction.Response.Match(new Response(actualResponse));
                    if (differences.Any())
                    {
                        test.Issues = differences;
                    }
                }
                tests.Add(test);
            }

            return tests;
        }

        internal List<Test> VerifyMessages(List<Message> messages)
        {
            var tests = new List<Test>();

            foreach (var message in messages)
            {
                var test = new Test { Description = message.Description };
                var verificationMessages = InvokeProviderStateHandler(message.ProviderStates);
                if (verificationMessages.Any())
                {
                    test.Issues = verificationMessages;
                }
                else
                {
                    object providedMessage = null;
                    try
                    {
                        providedMessage = _config.MessageProducer.Invoke(message.Description);
                    }
                    catch (PactVerificationException e)
                    {
                        test.Issues = new List<string> { $"Provider could not produce message {message.Description}: {e.Message}" };
                        tests.Add(test);
                        continue;
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
                        test.Issues = differences;
                    }
                }
                tests.Add(test);
            }

            return tests;
        }

        internal List<string> InvokeProviderStateHandler(List<ProviderState> providerStates)
        {
            var verificationMessages = new List<string>();

            if (providerStates == null)
            {
                return verificationMessages;
            }

            if (_config.ProviderStateHandler == null)
            {
                throw new PactException("Cannot verify this Pact contract because a ProviderStateHandler was not configured.");
            }

            foreach (var providerState in providerStates)
            {
                try
                {
                    _config.ProviderStateHandler.Invoke(providerState);
                }
                catch (PactVerificationException e)
                {
                    verificationMessages.Add($"Provider could not handle provider state \"{providerState.Name}\": {e.Message}");
                }
                catch
                {
                    throw new PactException("Exception occured while invoking ProviderStateHandler.");
                }
            }

            return verificationMessages;
        }

        internal async Task PublishVerificationResultsAsync(Contract pact, List<Test> tests)
        {
            if (string.IsNullOrWhiteSpace(_config.ProviderVersion))
            {
                throw new PactException("ProviderVersion should be configured to be able to publish verification results.");
            }

            var failureCount = tests.Count(t => t.Status == "failed");

            var testResults = new TestResults
            {
                Summary = new Summary { TestCount = tests.Count(), FailureCount = failureCount },
                Tests = tests
            };

            var verificationResults = new VerificationResults
            {
                ProviderName = pact.Provider.Name,
                ProviderApplicationVersion = _config.ProviderVersion,
                Success = failureCount == 0,
                VerificationDate = DateTime.UtcNow.ToString("u"),
                TestResults = testResults
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
