using ComPact.Mock.Consumer;
using ComPact.Models.V3;
using ComPact.Tests.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComPact.UnitTests.Mock.Consumer
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

            var config = new MockConsumerConfig
            {
                PactBrokerClient = new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }
            };

            var mockConsumer = new MockConsumer(config);

            var stringContent = await mockConsumer.GetPactFromBroker("some/path");

            Assert.IsNotNull(stringContent);
            Assert.IsNotNull(JsonConvert.DeserializeObject<Contract>(stringContent));
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

            var config = new MockConsumerConfig
            {
                PactBrokerClient = new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }
            };

            var mockConsumer = new MockConsumer(config);

            try
            {
                await mockConsumer.GetPactFromBroker("some/path");
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

            var config = new MockConsumerConfig
            {
                PactBrokerClient = new HttpClient(fakePactBrokerMessageHandler)
            };

            var mockConsumer = new MockConsumer(config);

            try
            {
                await mockConsumer.GetPactFromBroker("some/path");
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

            var config = new MockConsumerConfig
            {
                PactBrokerClient = new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }
            };

            var mockConsumer = new MockConsumer(config);

            try
            {
                await mockConsumer.GetPactFromBroker("some/path");
            }
            catch (PactException e)
            {
                Assert.AreEqual("Pact cannot be retrieved using the provided Pact Broker Client: Something went wrong.", e.Message);
                throw;
            }
        }
    }
}
