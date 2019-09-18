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
using ComPact.Exceptions;

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
            var pactVerifier = new PactVerifier(config);

            await pactVerifier.PublishVerificationResultsAsync(_pact, new List<Test> { new Test { Description = "test1" } });

            var sentVerificationResults = JsonConvert.DeserializeObject<VerificationResults>(fakeHttpMessageHandler.SentRequestContents.First().Value);
            Assert.IsTrue(sentVerificationResults.Success);
            Assert.AreEqual(config.ProviderVersion, sentVerificationResults.ProviderApplicationVersion);
            Assert.AreEqual(_pact.Provider.Name, sentVerificationResults.ProviderName);
            Assert.AreEqual(1, sentVerificationResults.TestResults.Summary.TestCount);
            Assert.AreEqual(0, sentVerificationResults.TestResults.Summary.FailureCount);
            Assert.AreEqual("test1", sentVerificationResults.TestResults.Tests.First().Description);
            Assert.AreEqual("passed", sentVerificationResults.TestResults.Tests.First().Status);
        }

        [TestMethod]
        public async Task PublishingFailedTests()
        {
            var fakeHttpMessageHandler = new FakePactBrokerMessageHandler();

            var config = new PactVerifierConfig
            {
                ProviderVersion = "1.0",
                PactBrokerClient = new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://local-pact-broker") }
            };
            var pactVerifier = new PactVerifier(config);

            await pactVerifier.PublishVerificationResultsAsync(_pact, new List<Test>
            {
                new Test { Description = "test1" },
                new Test { Description = "test2", Issues = new List<string> { "Something failed" } }
            });

            var sentVerificationResults = JsonConvert.DeserializeObject<VerificationResults>(fakeHttpMessageHandler.SentRequestContents.First().Value);
            Assert.IsFalse(sentVerificationResults.Success);
            Assert.AreEqual(config.ProviderVersion, sentVerificationResults.ProviderApplicationVersion);
            Assert.AreEqual(_pact.Provider.Name, sentVerificationResults.ProviderName);
            Assert.AreEqual(2, sentVerificationResults.TestResults.Summary.TestCount);
            Assert.AreEqual(1, sentVerificationResults.TestResults.Summary.FailureCount);
            Assert.AreEqual("passed", sentVerificationResults.TestResults.Tests.First(t => t.Description == "test1").Status);
            Assert.AreEqual("failed", sentVerificationResults.TestResults.Tests.First(t => t.Description == "test2").Status);
            Assert.AreEqual("Something failed", sentVerificationResults.TestResults.Tests.First(t => t.Description == "test2").Issues.First());
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
            var pactVerifier = new PactVerifier(config);

            try
            {
                await pactVerifier.PublishVerificationResultsAsync(_pact, new List<Test>());
            }
            catch (PactException e)
            {
                Assert.AreEqual("Publishing verification results failed. Pact Broker returned NotFound", e.Message);
                throw;
            }
        }
    }
}
