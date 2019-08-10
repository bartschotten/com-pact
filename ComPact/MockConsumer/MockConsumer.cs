using ComPact.Models;
using System;
using RestSharp;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace ComPact.MockConsumer
{
    public class MockConsumer
    {
        private readonly RestClient _client;

        public MockConsumer(string baseUrl)
        {
            _client = new RestClient(baseUrl);
        }

        public void VerifyPact(string filePath)
        {
            var pactFile = File.ReadAllText(filePath);

            var pact = JsonConvert.DeserializeObject<PactV2>(pactFile);

            foreach(var interaction in pact.Interactions)
            {
                var providerStatesRequest = new RestRequest("provider-states", RestSharp.Method.POST).AddJsonBody(new { State = interaction.ProviderState });
                _client.Execute(providerStatesRequest);
                var restRequest = interaction.Request.ToRestRequest();
                var actualResponse = _client.Execute(restRequest);
                var differences = interaction.Response.Match(new Response(actualResponse));
                if (differences.Any())
                {
                    throw new PactException(string.Join(Environment.NewLine, differences));
                }
            }
        }
    }
}
