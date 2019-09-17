using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ComPact.Exceptions;
using ComPact.ProviderTests.TestSupport;
using ComPact.Tests.Shared;
using ComPact.Verifier;
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

            var pactVerifier = new PactVerifier(new PactVerifierConfig { ProviderBaseUrl = baseUrl });

            var cts = new CancellationTokenSource();

            var hostTask = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseUrls(baseUrl)
                .UseStartup<TestStartup>()
                .Build().RunAsync(cts.Token);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            await pactVerifier.VerifyPactAsync(pactDir + "recipe-consumer-recipe-service.json");

            cts.Cancel();
            await hostTask;
        }

        [TestMethod]
        public async Task ShouldVerifyMessagePact()
        {
            var messageSender = new MessageSender();

            var config = new PactVerifierConfig
            {
                MessageProducer = messageSender.Send
            };

            var pactVerifier = new PactVerifier(config);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            await pactVerifier.VerifyPactAsync(pactDir + "messageConsumer-messageProvider.json");
        }

        [TestMethod]
        public async Task ShouldVerifyMessagePactWhenSerializedJsonIsReturned()
        {
            var messageSender = new MessageSender();

            var config = new PactVerifierConfig
            {
                MessageProducer = (p, d) => JsonConvert.SerializeObject(messageSender.Send(p, d))
            };

            var pactVerifier = new PactVerifier(config);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            await pactVerifier.VerifyPactAsync(pactDir + "messageConsumer-messageProvider.json");
        }

        [TestMethod]
        [ExpectedException(typeof(PactVerificationException))]
        public async Task VerificationForMessagePactShouldFailWhenWrongMessageIsReturned()
        {
            var config = new PactVerifierConfig
            {
                MessageProducer = (p, d) => null
            };

            var pactVerifier = new PactVerifier(config);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            try
            {
                await pactVerifier.VerifyPactAsync(pactDir + "messageConsumer-messageProvider.json");
            }
            catch (PactVerificationException e)
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

            var config = new PactVerifierConfig
            {
                MessageProducer = messageSender.Send,
                ProviderVersion = "1.0",
                PublishVerificationResults = true,
                PactBrokerClient = new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292")}
            };

            var pactVerifier = new PactVerifier(config);

            await pactVerifier.VerifyPactAsync("pacts/provider/messageProvider/consumer/messageConsumer/latest");

            var sentVerificationResults = JsonConvert.DeserializeObject<VerificationResults>(fakePactBrokerMessageHandler.SentRequestContents.First().Value);
            Assert.IsTrue(sentVerificationResults.Success);
        }
    }
}
