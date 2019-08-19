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
            string pactFile;

            try
            {
                pactFile = File.ReadAllText(filePath);
            }
            catch
            {
                throw new PactException($"Could not read file at {filePath}");
            }

            Models.V3.Contract pact = null;

            try
            {
                var contractWithSomeVersion = JsonConvert.DeserializeObject<ContractWithSomeVersion>(pactFile);
                if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Three)
                {
                    pact = JsonConvert.DeserializeObject<Models.V3.Contract>(pactFile);
                }
                else if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Two)
                {
                    var pactV2 = JsonConvert.DeserializeObject<Models.V2.Contract>(pactFile);
                    pact = new Models.V3.Contract(pactV2);
                }
                else if (contractWithSomeVersion.Metadata.GetVersion() == SpecificationVersion.Unsupported)
                {
                    throw new PactException("Pact specification version is not supported.");
                }
            }
            catch
            {
                throw new PactException("File was not recognized as a valid Pact contract.");
            }

            foreach(var interaction in pact.Interactions)
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
