using System;
using RestSharp;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using ComPact.Models;
using ComPact.Models.V3;

namespace ComPact.Mock.Consumer
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

            var pactV3 = JsonConvert.DeserializeObject<Models.V3.Contract>(pactFile);
            if (pactV3.Metadata.GetVersion() == SpecificationVersion.Two)
            {
                var pactV2 = JsonConvert.DeserializeObject<Models.V2.Contract>(pactFile);
                pactV3 = new Models.V3.Contract(pactV2);
            }
            else if (pactV3.Metadata.GetVersion() == SpecificationVersion.Unsupported)
            {
                throw new PactException("Pact specification version is not supported.");
            }

            foreach(var interaction in pactV3.Interactions)
            {
                var providerStatesRequest = new RestRequest("provider-states", RestSharp.Method.POST).AddJsonBody(interaction.ProviderState);
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
