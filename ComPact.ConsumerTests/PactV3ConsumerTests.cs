using ComPact.Builders;
using ComPact.Builders.V3;
using ComPact.ConsumerTests.TestSupport;
using ComPact.Exceptions;
using ComPact.Models;
using ComPact.Tests.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ComPact.ConsumerTests
{
    [TestClass]
    public class PactV3ConsumerTests
    {
        [TestMethod]
        public async Task ShouldMatchRequest()
        {
            var url = "http://localhost:9396";

            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler();
            fakePactBrokerMessageHandler.Configure(HttpMethod.Put, "http://localhost:9292").RespondsWith(HttpStatusCode.Created);
            var publisher = new PactPublisher(new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }, "1.0", "local");

            var builder = new PactBuilder("V3-consumer", "V3-provider", url, publisher);

            var recipeId = Guid.Parse("2860dedb-a193-425f-b73e-ef02db0aa8cf");

            var ingredient = Some.Object.With(
                                Some.Element.Named("name").Like("Salt"),
                                Some.Element.Named("amount").Like(5.5),
                                Some.Element.Named("unit").Like("gram"));

            builder.SetUp(Pact.Interaction
                .Given(new ProviderState { Name = $"There is a recipe with id `{recipeId}`" })
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
            Assert.AreEqual("V3-consumer",
                JsonConvert.DeserializeObject<Contract>(fakePactBrokerMessageHandler
                        .GetStatus(HttpMethod.Put, "http://localhost:9292").SentRequestContents.First().Value).Consumer
                    .Name);
            Assert.IsTrue(fakePactBrokerMessageHandler.GetStatus(HttpMethod.Put, "http://localhost:9292")
                .SentRequestContents.Last().Key.Contains("local"));

            // Check if pact has been written to project directory.
            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            var pactFile = File.ReadAllText(pactDir + "V3-consumer-V3-provider.json");
            Assert.AreEqual("V3-consumer", JsonConvert.DeserializeObject<Contract>(pactFile).Consumer.Name);
            File.Delete(pactDir + "V3-consumer-V3-provider.json");
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task ShouldNotBuildWhenNotAllInteractionsHaveBeenMatched()
        {
            var url = "http://localhost:9397";

            var builder = new PactBuilder("test-consumer", "test-provider", url);

            builder.SetUp(Pact.Interaction
                .UponReceiving("a request")
                .With(Pact.Request
                    .WithHeader("Accept", "application/json")
                    .WithMethod(Method.GET)
                    .WithPath("/testpath"))
                .WillRespondWith(Pact.Response
                    .WithStatus(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(Pact.JsonContent.Empty())));

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
        public async Task ShouldReturnLastResponseWhenMultipleRequestsMatch()
        {
            var url = "http://localhost:9398";

            var builder = new PactBuilder("test-consumer", "test-provider", url);

            builder.SetUp(Pact.Interaction
                .UponReceiving("a request")
                .With(Pact.Request
                    .WithHeader("Accept", "application/json")
                    .WithMethod(Method.GET)
                    .WithPath("/testpath"))
                .WillRespondWith(Pact.Response
                    .WithStatus(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(Pact.JsonContent.Empty())));

            builder.SetUp(Pact.Interaction
                .Given(new ProviderState { Name = "some state" })
                .UponReceiving("a request")
                .With(Pact.Request
                    .WithHeader("Accept", "application/json")
                    .WithMethod(Method.GET)
                    .WithPath("/testpath"))
                .WillRespondWith(Pact.Response
                    .WithStatus(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(Pact.JsonContent.With(Some.Element.WithTheExactValue("test")))));

            var httpClient = new HttpClient { BaseAddress = new Uri(url) };
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await httpClient.GetAsync("testpath");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseContent.Contains("test"));
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task ShouldNotBuildWhenNoInteractionsHaveBeenSetUp()
        {
            var url = "http://localhost:9399";

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

        [TestMethod]
        public async Task ShouldWorkWithEmptyResponse()
        {
            var url = "http://localhost:9400";

            var builder = new PactBuilder("V3-consumer", "V3-provider", url);

            builder.SetUp(Pact.Interaction
                .UponReceiving("too many requests")
                .With(Pact.Request
                    .WithMethod(Method.GET)
                    .WithPath($"/test"))
                .WillRespondWith(Pact.Response
                    .WithStatus(429)
                    ));

            using (var client = new HttpClient { BaseAddress = new Uri(url) })
            {
                var response = await client.GetAsync($"test");
                Assert.AreEqual(System.Net.HttpStatusCode.TooManyRequests, response.StatusCode);
            }

            await builder.BuildAsync();

            // Check if pact has been written to project directory.
            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            var pactFile = File.ReadAllText(pactDir + "V3-consumer-V3-provider.json");
            Assert.AreEqual("V3-consumer", JsonConvert.DeserializeObject<Contract>(pactFile).Consumer.Name);
            File.Delete(pactDir + "V3-consumer-V3-provider.json");
        }
    }
}
