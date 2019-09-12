using ComPact.Models.V3;
using ComPact.Models;
using ComPact.Tests.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using ComPact.Verifier;

namespace ComPact.UnitTests.Verifier
{
    [TestClass]
    public class PublishVerificationResultsTests
    {
        private Contract _pact = new Contract { Consumer = new Pacticipant { Name = "consumer" }, Provider = new Pacticipant { Name = "provider" } };

        [TestMethod]
        public async Task SuccessfulPublication()
        {
            var fakeHttpMessageHandler = new FakePactBrokerMessageHandler();

            var config = new PactVerifierConfig
            {
                ProviderVersion = "1.0",
                PactBrokerClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://local-pact-broker") }
            };
            var mockConsumer = new PactVerifier(config);

            await mockConsumer.PublishVerificationResultsAsync(_pact, new List<FailedInteraction>());

            var sentVerificationResults = JsonConvert.DeserializeObject<VerificationResults>(fakeHttpMessageHandler.SentRequestContents.First().Value);
            Assert.IsTrue(sentVerificationResults.Success);
            Assert.AreEqual(config.ProviderVersion, sentVerificationResults.ProviderApplicationVersion);
            Assert.AreEqual(_pact.Provider.Name, sentVerificationResults.ProviderName);
            Assert.IsFalse(sentVerificationResults.FailedInteractions.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task PactBrokerReturnsNonSuccessStatusCode()
        {
            var fakeHttpMessageHandler = new FakePactBrokerMessageHandler { StatusCodeToReturn = System.Net.HttpStatusCode.NotFound };

            var config = new PactVerifierConfig
            {
                ProviderVersion = "1.0",
                PactBrokerClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://local-pact-broker") }
            };
            var mockConsumer = new PactVerifier(config);

            try
            {
                await mockConsumer.PublishVerificationResultsAsync(_pact, new List<FailedInteraction>());
            }
            catch (PactException e)
            {
                Assert.AreEqual("Publishing verification results failed. Pact Broker returned NotFound", e.Message);
                throw;
            }
        }
    }
}
