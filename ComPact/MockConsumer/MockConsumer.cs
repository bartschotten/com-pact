using ComPact.Models;
using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using System.IO;
using Newtonsoft.Json;

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
            }
        }
    }
}
