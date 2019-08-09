using ComPact.Builders;
using ComPact.Matchers;
using ComPact.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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

            var builder = new PactBuilderV2("test-consumer", "test-producer", url, NullLogger.Instance);

            var recipeId = Guid.Parse("2860dedb-a193-425f-b73e-ef02db0aa8cf");

            var ingredient = Some.Object.With(
                                Some.Element.Named("name").Like("Salt"),
                                Some.Element.Named("amount").Like(5.5),
                                Some.Element.Named("unit").Like("gram"));

            builder.SetupInteraction(new InteractionV2Builder()
                .Given($"There is a recipe with id `{recipeId}`")
                .UponReceiving("a request")
                .With(Pact.Request
                    .WithHeader("Accept", "application/json")
                    .WithMethod(Method.GET)
                    .ToPath($"/api/recipes/{recipeId}"))
                .WillRespondWith(Pact.Response
                    .WithStatus(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(Pact.ResponseBody.With(
                        Some.Element.Named("name").Like("A Recipe"),
                        Some.Element.Named("instructions").Like("Mix it up"),
                        Some.Array.Named("ingredients").Of(ingredient)
                    ))));

            var client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await client.GetAsync($"api/recipes/{recipeId}");

            Assert.IsTrue(response.IsSuccessStatusCode);

            builder.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldNotBuildWhenNotAllInteractionsHaveBeenMatched()
        {
            var url = "http://localhost:9393";

            var builder = new PactBuilderV2("test-consumer", "test-producer", url, NullLogger.Instance);

            builder.SetupInteraction(new InteractionV2Builder()
                .UponReceiving("a request")
                .With(Pact.Request
                    .WithHeader("Accept", "application/json")
                    .WithMethod(Method.GET)
                    .ToPath("/testpath"))
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

            var builder = new PactBuilderV2("test-consumer", "test-producer", url, NullLogger.Instance);

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
