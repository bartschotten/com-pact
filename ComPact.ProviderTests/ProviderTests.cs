using System;
using System.IO;
using System.Threading;
using ComPact.Mock.Consumer;
using ComPact.ProviderTests.TestSupport;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComPact.ProviderTests
{
    [TestClass]
    public class ProviderTests
    {
        [TestMethod]
        public void ShouldVerifyPact()
        {
            var baseUrl = "http://localhost:9494";

            var mockConsumer = new MockConsumer(baseUrl);

            var cts = new CancellationTokenSource();

            WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseUrls(baseUrl)
                .UseStartup<TestStartup>()
                .Build().RunAsync(cts.Token);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            mockConsumer.VerifyPact(pactDir + "recipe-consumer-recipe-service.json");

            cts.Cancel();
        }

        [TestMethod]
        public void ShouldVerifyMessagePact()
        {
            FakeRecipeRepository recipeRepository = new FakeRecipeRepository();

            var recipeAddedProducer = new RecipeAddedProducer(recipeRepository);

            var mockConsumer = new MockConsumer(p => new MessageProviderStateHandler(recipeRepository).Handle(p), recipeAddedProducer.Send);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            mockConsumer.VerifyPact(pactDir + "messageConsumer-messageProvider.json");
        }
    }
}
