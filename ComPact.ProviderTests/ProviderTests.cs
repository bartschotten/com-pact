using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ComPact.Mock.Consumer;
using ComPact.ProviderTests.TestSupport;
using ComPact.Tests.Shared;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ComPact.ProviderTests
{
    [TestClass]
    public class ProviderTests
    {
        [TestMethod]
        public async Task ShouldVerifyPact()
        {
            var baseUrl = "http://localhost:9494";

            var mockConsumer = new MockConsumer(new MockConsumerConfig { ProviderBaseUrl = baseUrl });

            var cts = new CancellationTokenSource();

            var hostTask = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseUrls(baseUrl)
                .UseStartup<TestStartup>()
                .Build().RunAsync(cts.Token);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            await mockConsumer.VerifyPactAsync(pactDir + "recipe-consumer-recipe-service.json");

            cts.Cancel();
            await hostTask;
        }

        [TestMethod]
        public async Task ShouldVerifyMessagePact()
        {
            var messageSender = new MessageSender();

            var config = new MockConsumerConfig
            {
                MessageProducer = messageSender.Send
            };

            var mockConsumer = new MockConsumer(config);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            await mockConsumer.VerifyPactAsync(pactDir + "messageConsumer-messageProvider.json");
        }

        [TestMethod]
        public async Task ShouldVerifyMessagePactWhenSerializedJsonIsReturned()
        {
            var messageSender = new MessageSender();

            var config = new MockConsumerConfig
            {
                MessageProducer = (p, d) => JsonConvert.SerializeObject(messageSender.Send(p, d))
            };

            var mockConsumer = new MockConsumer(config);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            await mockConsumer.VerifyPactAsync(pactDir + "messageConsumer-messageProvider.json");
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task VerificationForMessagePactShouldFailWhenWrongMessageIsReturned()
        {
            var config = new MockConsumerConfig
            {
                MessageProducer = (p, d) => null
            };

            var mockConsumer = new MockConsumer(config);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            try
            {
                await mockConsumer.VerifyPactAsync(pactDir + "messageConsumer-messageProvider.json");
            }
            catch (PactException e)
            {
                Assert.IsTrue(e.Message.Contains("Expected body or contents to be present, but was not"));
                throw;
            }
        }

        [TestMethod]
        public async Task ShouldPublishVerificationResults()
        {
            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            var pactFileToReturn = File.ReadAllText(pactDir + "messageConsumer-messageProvider.json");
            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler
            {
                ObjectToReturn = JsonConvert.DeserializeObject(pactFileToReturn)
            };

            var messageSender = new MessageSender();

            var config = new MockConsumerConfig
            {
                MessageProducer = messageSender.Send,
                ProviderVersion = "1.0",
                PublishVerificationResults = true,
                PactBrokerClient = new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292")}
            };

            var mockConsumer = new MockConsumer(config);

            await mockConsumer.VerifyPactAsync("pacts/provider/messageProvider/consumer/messageConsumer/latest");

            var sentVerificationResults = JsonConvert.DeserializeObject<VerificationResults>(fakePactBrokerMessageHandler.SentRequestContents.First().Value);
            Assert.IsTrue(sentVerificationResults.Success);
        }
    }
}
