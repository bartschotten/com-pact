using ComPact.Builders;
using ComPact.Models;
using ComPact.Models.V3;
using ComPact.Tests.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComPact.UnitTests.Builders
{
    [TestClass]
    public class PactPublisherTests
    {
        private Contract _pact = new Contract { Consumer = new Pacticipant { Name = "consumer" }, Provider = new Pacticipant { Name = "provider" } };

        [TestMethod]
        public async Task SuccessfulPublicationAndTagging()
        {
            var fakeHttpMessageHandler = new FakePactBrokerMessageHandler();

            var pactPublisher = new PactPublisher(new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://local-pact-broker") }, "1.0", "local");

            await pactPublisher.PublishAsync(_pact);

            Assert.AreEqual("http://local-pact-broker/pacts/provider/provider/consumer/consumer/version/1.0", fakeHttpMessageHandler.SentRequestContents.First().Key);
            var sentContract = JsonConvert.DeserializeObject<Contract>(fakeHttpMessageHandler.SentRequestContents.First().Value);
            Assert.IsNotNull(sentContract);
            Assert.AreEqual(_pact.Consumer.Name, sentContract.Consumer.Name);

            Assert.AreEqual("http://local-pact-broker/pacticipants/consumer/versions/1.0/tags/local", fakeHttpMessageHandler.SentRequestContents.Last().Key);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ClientCannotBeNull()
        {
            try
            {
                new PactPublisher(null, "1.0", "local");
            }
            catch (PactException e)
            {
                Assert.AreEqual("A pactBrokerClient with at least a BaseAddress should be configured to be able to publish contracts.", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void BaseAddressMustBeProvided()
        {
            try
            {
                new PactPublisher(new HttpClient(), "1.0", "local");
            }
            catch (PactException e)
            {
                Assert.AreEqual("A pactBrokerClient with at least a BaseAddress should be configured to be able to publish contracts.", e.Message);
                throw;
            }
        }

        [DataTestMethod]
        [ExpectedException(typeof(PactException))]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void ConsumerVersionMustBeNotBeNullOrWhitespace(string version)
        {
            try
            {
                new PactPublisher(new HttpClient() { BaseAddress = new Uri("http://local-pact-broker") }, version, "local");
            }
            catch (PactException e)
            {
                Assert.AreEqual("ConsumerVersion should be configured to be able to publish contracts.", e.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task TaggingIsOptional()
        {
            var fakeHttpMessageHandler = new FakePactBrokerMessageHandler();

            var pactPublisher = new PactPublisher(new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://local-pact-broker") }, "1.0");

            await pactPublisher.PublishAsync(_pact);

            Assert.AreEqual("http://local-pact-broker/pacts/provider/provider/consumer/consumer/version/1.0", fakeHttpMessageHandler.SentRequestContents.First().Key);
            var sentContract = JsonConvert.DeserializeObject<Contract>(fakeHttpMessageHandler.SentRequestContents.First().Value);
            Assert.IsNotNull(sentContract);
            Assert.AreEqual(_pact.Consumer.Name, sentContract.Consumer.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task ShouldThrowWhenCallNotSuccessful()
        {
            var fakeHttpMessageHandler = new FakePactBrokerMessageHandler() { StatusCodeToReturn = System.Net.HttpStatusCode.BadRequest };

            var pactPublisher = new PactPublisher(new HttpClient(fakeHttpMessageHandler) { BaseAddress = new Uri("http://local-pact-broker") }, "1.0");

            try
            {
                await pactPublisher.PublishAsync(_pact);
            }
            catch (PactException e)
            {
                Assert.AreEqual("Publishing contract failed. Pact Broker returned BadRequest", e.Message);
                throw;
            }
        }
    }
}
