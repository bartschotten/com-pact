using ComPact.Builders;
using ComPact.Builders.V3;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ComPact.ConsumerTests
{
    [TestClass]
    public class PactV3ConsumerTests
    {
        [TestMethod]
        public async Task ShouldMatchRequest()
        {
            var url = "http://localhost:9393";

            var publisher = new PactPublisher(new HttpClient { BaseAddress = new Uri("http://localhost:9292") }, "1.0", "local");

            var builder = new PactBuilder("V3-consumer", "V3-producer", url, null, publisher);

            var recipeId = Guid.Parse("2860dedb-a193-425f-b73e-ef02db0aa8cf");

            var ingredient = Some.Object.With(
                                Some.Element.Named("name").Like("Salt"),
                                Some.Element.Named("amount").Like(5.5),
                                Some.Element.Named("unit").Like("gram"));

            builder.SetupInteraction(new InteractionBuilder()
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
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public async Task ShouldNotBuildWhenNotAllInteractionsHaveBeenMatched()
        {
            var url = "http://localhost:9393";

            var builder = new PactBuilder("test-consumer", "test-producer", url);

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

            var builder = new PactBuilder("test-consumer", "test-producer", url);

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
