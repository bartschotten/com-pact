using ComPact.Builders.V2;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

            var builder = new PactBuilder("V2-consumer", "V2-producer", url);

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
                    .WithBody(Pact.ResponseBody.With(
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

            builder.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldNotBuildWhenNotAllInteractionsHaveBeenMatched()
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
                    .WithBody(Pact.ResponseBody.Empty())));

            try
            {
                builder.Build();
            }
            catch (PactException e)
            {
                Assert.AreEqual("Cannot build pact. Not all mocked interactions have been called.", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldNotBuildWhenNoInteractionsHaveBeenSetUp()
        {
            var url = "http://localhost:9393";

            var builder = new PactBuilder("test-consumer", "test-producer", url);

            try
            {
                builder.Build();
            }
            catch (PactException e)
            {
                Assert.AreEqual("Cannot build pact. No interactions.", e.Message);
                throw;
            }
        }
    }
}
