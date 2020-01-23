using System;
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
            string pactContent = null;
            string publishVerificationResultsUrl = null;

            if (_config.PactBrokerClient != null)
            {
                var pactBrokerResults = await GetPactFromBroker(_config.PactBrokerClient, path);
                pactContent = pactBrokerResults.PactContent;
                publishVerificationResultsUrl = pactBrokerResults.PublishVerificationResultsUrl;
            }
            else
            {
                pactContent = ReadPactFile(path);
            }

            Contract pact = DeserializePactContent(pactContent);

            var tests = new List<Test>();

            if (pact.Interactions != null)
            {
                if (_config.ProviderBaseUrl == null && _config.ProviderHttpClient?.BaseAddress == null)
                {
                    throw new PactException("Could not verify pacts. Please configure a ProviderBaseUrl or a pre-configured ProviderHttpClient with at least a BaseAddress.");
                }

                var client = _config.ProviderHttpClient ?? new HttpClient {BaseAddress = new Uri(_config.ProviderBaseUrl) };
                tests.AddRange(await VerifyInteractions(pact.Interactions, client.SendAsync, _config.ProviderStateHandler));
            }

            if (pact.Messages != null)
            {
                tests.AddRange(VerifyMessages(pact.Messages, _config.ProviderStateHandler, _config.MessageProducer));
            }

            if (_config.PublishVerificationResults)
            {
                await PublishVerificationResultsAsync(pact, tests, _config.ProviderVersion, _config.PactBrokerClient, publishVerificationResultsUrl);
            }

            if (tests.Any(t => t.Status == "failed"))
            {
                throw new PactVerificationException(string.Join(Environment.NewLine, tests.Select(f => f.ToTestMessageString())));
            }
        }

        internal static async Task<PactBrokerResults> GetPactFromBroker(HttpClient pactBrokerClient, string path)
        {
            if (pactBrokerClient.BaseAddress == null)
            {
                throw new PactException("A PactBrokerClient with at least a BaseAddress should be configured to be able to retrieve contracts.");
            }

            HttpResponseMessage response;
            try
            {
                response = await pactBrokerClient.GetAsync(path);
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
            var publishVerificationResultsUrl = pactJObject.SelectToken("_links.pb:publish-verification-results.href").Value<string>();

            return new PactBrokerResults
            {
                PactContent = stringContent,
                PublishVerificationResultsUrl = publishVerificationResultsUrl
            };
        }

        internal static string ReadPactFile(string filePath)
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

        internal static Contract DeserializePactContent(string pactContent)
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

        internal async static Task<List<Test>> VerifyInteractions(List<Interaction> interactions, Func<HttpRequestMessage, Task<HttpResponseMessage>> providerClient, Action<ProviderState> providerStateHandler)
        {
            var tests = new List<Test>();

            foreach (var interaction in interactions)
            {
                var test = new Test { Description = interaction.Description };
                var verificationMessages = InvokeProviderStateHandler(interaction.ProviderStates, providerStateHandler);
                if (verificationMessages.Any())
                {
                    test.Issues = verificationMessages;
                }
                else
                {
                    var httpRequestMessage = interaction.Request.ToHttpRequestMessage();
                    var actualResponse = await providerClient.Invoke(httpRequestMessage);
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

        internal static List<Test> VerifyMessages(List<Message> messages, Action<ProviderState> providerStateHandler, Func<string, object> messageProducer)
        {
            var tests = new List<Test>();

            foreach (var message in messages)
            {
                var test = new Test { Description = message.Description };
                var verificationMessages = InvokeProviderStateHandler(message.ProviderStates, providerStateHandler);
                if (verificationMessages.Any())
                {
                    test.Issues = verificationMessages;
                }
                else
                {
                    object providedMessage = null;
                    try
                    {
                        providedMessage = messageProducer.Invoke(message.Description);
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

        internal static List<string> InvokeProviderStateHandler(List<ProviderState> providerStates, Action<ProviderState> providerStateHandler)
        {
            var verificationMessages = new List<string>();

            if (providerStates == null)
            {
                return verificationMessages;
            }

            if (providerStateHandler == null)
            {
                throw new PactException("Cannot verify this Pact contract because a ProviderStateHandler was not configured.");
            }

            foreach (var providerState in providerStates)
            {
                try
                {
                    providerStateHandler.Invoke(providerState);
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

        internal static async Task PublishVerificationResultsAsync(Contract pact, List<Test> tests, string providerVersion, HttpClient pactBrokerClient, string publishVerificationResultsUrl)
        {
            if (string.IsNullOrWhiteSpace(providerVersion))
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
                ProviderApplicationVersion = providerVersion,
                Success = failureCount == 0,
                VerificationDate = DateTime.UtcNow.ToString("u"),
                TestResults = testResults
            };
            var content = new StringContent(JsonConvert.SerializeObject(verificationResults), Encoding.UTF8, "application/json");

            var response = await pactBrokerClient.PostAsync(publishVerificationResultsUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new PactException("Publishing verification results failed. Pact Broker returned " + response.StatusCode);
            }
        }
    }

    internal class PactBrokerResults
    {
        internal string PactContent { get; set; }
        internal string PublishVerificationResultsUrl { get; set; }
    }
}
