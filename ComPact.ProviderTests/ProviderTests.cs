using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ComPact.Exceptions;
using ComPact.ProviderTests.TestSupport;
using ComPact.Tests.Shared;
using ComPact.Verifier;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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

            var recipeRepository = new FakeRecipeRepository();
            var providerStateHandler = new ProviderStateHandler(recipeRepository);

            var pactVerifier = new PactVerifier(new PactVerifierConfig { ProviderBaseUrl = baseUrl, ProviderStateHandler = providerStateHandler.Handle });

            var cts = new CancellationTokenSource();

            var hostTask = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseUrls(baseUrl)
                .ConfigureServices(services =>
                {
                    services.Add(new ServiceDescriptor(typeof(IRecipeRepository), recipeRepository));
                })
                .UseStartup<TestStartup>()
                .Build().RunAsync(cts.Token);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            await pactVerifier.VerifyPactAsync(pactDir + "recipe-consumer-recipe-service.json");

            cts.Cancel();
            await hostTask;
        }

        [TestMethod]
        public async Task ShouldVerifyPact_ProviderHttpClient()
        {
            var baseUrl = "http://localhost:9494";

            var recipeRepository = new FakeRecipeRepository();
            var providerStateHandler = new ProviderStateHandler(recipeRepository);
            var cts = new CancellationTokenSource();
            Task hostTask;

            using (var client = new HttpClient {BaseAddress = new Uri(baseUrl)})
            {
                var pactVerifier = new PactVerifier(new PactVerifierConfig
                    {ProviderHttpClient = client, ProviderStateHandler = providerStateHandler.Handle});

                hostTask = WebHost.CreateDefaultBuilder()
                    .UseKestrel()
                    .UseUrls(baseUrl)
                    .ConfigureServices(services =>
                    {
                        services.Add(new ServiceDescriptor(typeof(IRecipeRepository), recipeRepository));
                    })
                    .UseStartup<TestStartup>()
                    .Build().RunAsync(cts.Token);

                var buildDirectory = AppContext.BaseDirectory;
                var pactDir =
                    Path.GetFullPath(
                        $"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
                await pactVerifier.VerifyPactAsync(pactDir + "recipe-consumer-recipe-service.json");
            }

            cts.Cancel();
            await hostTask;
        }

        [TestMethod]
        public async Task ShouldVerifyMessagePact()
        {
            var recipeRepository = new FakeRecipeRepository();
            var providerStateHandler = new ProviderStateHandler(recipeRepository);
            var messageSender = new MessageSender(recipeRepository);

            var config = new PactVerifierConfig
            {
                ProviderStateHandler = providerStateHandler.Handle,
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
            var recipeRepository = new FakeRecipeRepository();
            var providerStateHandler = new ProviderStateHandler(recipeRepository);
            var messageSender = new MessageSender(recipeRepository);

            var config = new PactVerifierConfig
            {
                ProviderStateHandler = providerStateHandler.Handle,
                MessageProducer = (d) => JsonConvert.SerializeObject(messageSender.Send(d))
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
            var recipeRepository = new FakeRecipeRepository();
            var providerStateHandler = new ProviderStateHandler(recipeRepository);

            var config = new PactVerifierConfig
            {
                ProviderStateHandler = providerStateHandler.Handle,
                MessageProducer = (d) => null
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
            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler();

            fakePactBrokerMessageHandler
                .Configure(HttpMethod.Get,
                    "http://localhost:9292/pacts/provider/messageProvider/consumer/messageConsumer/latest")
                .RespondsWith(HttpStatusCode.Created).Returns(JsonConvert.DeserializeObject(pactFileToReturn));
            fakePactBrokerMessageHandler
                .Configure(HttpMethod.Post, "http://localhost:9292/publish/verification/results/path")
                .RespondsWith(HttpStatusCode.Created);

            var recipeRepository = new FakeRecipeRepository();
            var providerStateHandler = new ProviderStateHandler(recipeRepository);
            var messageSender = new MessageSender(recipeRepository);

            var config = new PactVerifierConfig
            {
                ProviderStateHandler = providerStateHandler.Handle,
                MessageProducer = messageSender.Send,
                ProviderVersion = "1.0",
                PublishVerificationResults = true,
                PactBrokerClient = new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292")}
            };

            var pactVerifier = new PactVerifier(config);

            await pactVerifier.VerifyPactAsync("pacts/provider/messageProvider/consumer/messageConsumer/latest");

            var sentVerificationResults = JsonConvert.DeserializeObject<VerificationResults>(
                fakePactBrokerMessageHandler.GetStatus(HttpMethod.Post,
                        "http://localhost:9292/publish/verification/results/path")
                    .SentRequestContents.First().Value);
            Assert.IsTrue(sentVerificationResults.Success);
            Assert.AreEqual(1, sentVerificationResults.TestResults.Summary.TestCount);
            Assert.AreEqual(0, sentVerificationResults.TestResults.Summary.FailureCount);
        }
    }
}
