using ComPact.Exceptions;
using ComPact.Models.V3;
using ComPact.Tests.Shared;
using ComPact.Verifier;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComPact.UnitTests.Verifier
{
    [TestClass]
    public class GetPactFromPactBrokerTests
    {
        [TestMethod]
        public async Task ShouldGetPactFromPactBroker()
        {
            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler
            {
                ObjectToReturn = new Contract()
            };

            var pactBrokerResults = await PactVerifier.GetPactFromBroker(new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }, "some/path");

            Assert.IsNotNull(pactBrokerResults);
            Assert.IsNotNull(JsonConvert.DeserializeObject<Contract>(pactBrokerResults.PactContent));
            Assert.IsNotNull(pactBrokerResults.PublishVerificationResultsUrl);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task ShouldThrowWhenResponseIsNotSuccessful()
        {
            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler
            {
                ObjectToReturn = new Contract(),
                StatusCodeToReturn = System.Net.HttpStatusCode.BadRequest
            };

            try
            {
                await PactVerifier.GetPactFromBroker(new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }, "some/path");
            }
            catch (PactException e)
            {
                Assert.AreEqual("Getting pact from Pact Broker failed. Pact Broker returned BadRequest", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task ShouldThrowWhenClientHasNoBaseAddress()
        {
            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler
            {
                ObjectToReturn = new Contract(),
            };

            try
            {
                await PactVerifier.GetPactFromBroker(new HttpClient(fakePactBrokerMessageHandler), "some/path");
            }
            catch (PactException e)
            {
                Assert.AreEqual("A PactBrokerClient with at least a BaseAddress should be configured to be able to retrieve contracts.", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task ShouldThrowWhenClientThrowsForAnyOtherReason()
        {
            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler
            {
                ObjectToReturn = new Contract(),
                ExceptionToThrow = new HttpRequestException("Something went wrong.")
            };

            try
            {
                await PactVerifier.GetPactFromBroker(new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }, "some/path");
            }
            catch (PactException e)
            {
                Assert.AreEqual("Pact cannot be retrieved using the provided Pact Broker Client: Something went wrong.", e.Message);
                throw;
            }
        }
    }
}
