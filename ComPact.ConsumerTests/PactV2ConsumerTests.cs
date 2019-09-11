using ComPact.Builders;
using ComPact.Builders.V2;
using ComPact.ConsumerTests.TestSupport;
using ComPact.Models;
using ComPact.Tests.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComPact.ConsumerTests
{
    [TestClass]
    public class PactV2ConsumerTests
    {
        [TestMethod]
        public async Task ShouldMatchRequest()
        {
            var url = "http://localhost:9393";

            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler();
            var publisher = new PactPublisher(new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }, "1.0", "local");

            var builder = new PactBuilder("V2-consumer", "V2-provider", url, publisher);

            var recipeId = Guid.Parse("2860dedb-a193-425f-b73e-ef02db0aa8cf");

            var ingredient = Some.Object.With(
                                Some.Element.Named("name").Like("Salt"),
                                Some.Element.Named("amount").Like(5.5),
                                Some.Element.Named("unit").Like("gram"));

            builder.SetupInteraction(new InteractionBuilder()
                .Given($"There is a recipe with id `{recipeId}`")
                .UponReceiving("a request")
                .With(Pact.Request
                    .WithHeader("Accept", "application/json")
                    .WithMethod(Method.GET)
                    .WithPath($"/api/recipes/{recipeId}"))
                .WillRespondWith(Pact.Response
                    .WithStatus(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(Pact.JsonContent.With(
                        Some.Element.Named("name").Like("A Recipe"),
                        Some.Element.Named("instructions").Like("Mix it up"),
                        Some.Array.Named("ingredients").Of(ingredient)
                    ))));

            using (var client = new HttpClient { BaseAddress = new Uri(url) })
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await client.GetAsync($"api/recipes/{recipeId}");

                Assert.IsTrue(response.IsSuccessStatusCode);
            }

            await builder.BuildAsync();

            // Check if pact has been published and tagged
            Assert.AreEqual("V2-consumer", JsonConvert.DeserializeObject<Contract>(fakePactBrokerMessageHandler.SentRequestContents.First().Value).Consumer.Name);
            Assert.IsTrue(fakePactBrokerMessageHandler.SentRequestContents.Last().Key.Contains("local"));

            // Check if pact has been written to project directory.
            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            var pactFile = File.ReadAllText(pactDir + "V2-consumer-V2-provider.json");
            Assert.AreEqual("V2-consumer", JsonConvert.DeserializeObject<Contract>(pactFile).Consumer.Name);
            File.Delete(pactDir + "V2-consumer-V2-provider.json");
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task ShouldNotBuildWhenNotAllInteractionsHaveBeenMatched()
        {
            var url = "http://localhost:9393";

            var builder = new PactBuilder("test-consumer", "test-provider", url);

            builder.SetupInteraction(new InteractionBuilder()
                .UponReceiving("a request")
                .With(Pact.Request
                    .WithHeader("Accept", "application/json")
                    .WithMethod(Method.GET)
                    .WithPath("/testpath"))
                .WillRespondWith(Pact.Response
                    .WithStatus(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(Pact.JsonContent.Empty())));

            var httpClient = new HttpClient { BaseAddress = new Uri(url) };
            var response = await httpClient.GetAsync("wrongPath");
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseContent.Contains("No matching response set up for this request."));

            try
            {
                await builder.BuildAsync();
            }
            catch (PactException e)
            {
                Assert.AreEqual("Cannot build pact. Not all mocked interactions have been called.", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task ShouldNotBuildWhenNoInteractionsHaveBeenSetUp()
        {
            var url = "http://localhost:9393";

            var builder = new PactBuilder("test-consumer", "test-provider", url);

            try
            {
                await builder.BuildAsync();
            }
            catch (PactException e)
            {
                Assert.AreEqual("Cannot build pact. No interactions.", e.Message);
                throw;
            }
        }
    }
}
