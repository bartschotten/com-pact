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
            FakeRecipeRepository recipeRepository = new FakeRecipeRepository();

            var recipeAddedProducer = new RecipeAddedProducer(recipeRepository);

            var config = new MockConsumerConfig
            {
                MessageProviderStateHandler = p => new MessageProviderStateHandler(recipeRepository).Handle(p),
                MessageProducer = recipeAddedProducer.Send
            };

            var mockConsumer = new MockConsumer(config);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            await mockConsumer.VerifyPactAsync(pactDir + "messageConsumer-messageProvider.json");
        }

        [TestMethod]
        public async Task ShouldPublishVerificationResults()
        {
            FakeRecipeRepository recipeRepository = new FakeRecipeRepository();

            var recipeAddedProducer = new RecipeAddedProducer(recipeRepository);

            //var fakeHttpMessageHandler = new FakeHttpMessageHandler();

            var config = new MockConsumerConfig
            {
                MessageProviderStateHandler = p => new MessageProviderStateHandler(recipeRepository).Handle(p),
                MessageProducer = recipeAddedProducer.Send,
                ProviderVersion = "1.0",
                PublishVerificationResults = true,
                PactBrokerClient = new HttpClient() { BaseAddress = new Uri("http://localhost:9292")}
            };

            var mockConsumer = new MockConsumer(config);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            await mockConsumer.VerifyPactAsync("pacts/provider/messageProvider/consumer/messageConsumer/latest");

            //var sentVerificationResults = JsonConvert.DeserializeObject<VerificationResults>(fakeHttpMessageHandler.SentRequestContents.First().Value);
            //Assert.IsTrue(sentVerificationResults.Success);
        }
    }
}
