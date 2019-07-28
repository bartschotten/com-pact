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

            builder.SetupInteraction(new InteractionV2Builder()
                .Given($"There is a recipe with id `{recipeId}`")
                .UponReceiving("a request")
                .With(new Request
                {
                    Headers = new Headers { { "Accept", "application/json" } },
                    Method = Method.GET,
                    Path = $"/api/recipes/{recipeId}"
                })
                .WillRespondWith(new Response
                {
                    Status = 200,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                    Body = new
                    {
                        name = Match.Type("A Recipe"),
                        instructions = Match.Type("Mix it up"),
                        ingredients = Match.MinType(new []
                        {
                            new
                            {
                                name = "Salt",
                                amount = 5.5,
                                unit = "gram"
                            }
                        }, 
                        1)
                    }
                }));

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
                .With(new Request
                {
                    Headers = new Headers { { "Accept", "application/json" } },
                    Method = Method.GET,
                    Path = "/testpath"
                })
                .WillRespondWith(new Response
                {
                    Status = 200,
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                    Body = new object { }
                }));

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
