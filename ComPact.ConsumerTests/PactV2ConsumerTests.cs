using ComPact.Builders;
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

            var interaction = new InteractionV2Builder()
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
                })
                .Build();

            builder.SetupInteraction(interaction);

            var client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await client.GetAsync("testpath");

            Assert.IsTrue(response.IsSuccessStatusCode);

            builder.Build();
        }
    }
}
