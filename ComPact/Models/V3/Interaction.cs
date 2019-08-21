using Newtonsoft.Json;
using System;

namespace ComPact.Models.V3
{
    internal class Interaction
    {
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        [JsonProperty("providerState")]
        public ProviderState ProviderState { get; set; }
        [JsonProperty("request")]
        public Request Request { get; set; } = new Request();
        [JsonProperty("response")]
        public Response Response { get; set; } = new Response();

        public Interaction() { }
        public Interaction(V2.Interaction interaction)
        {
            Description = interaction?.Description ?? throw new ArgumentNullException(nameof(interaction));
            ProviderState = new ProviderState { Name = interaction.ProviderState };
            Request = new Request(interaction.Request);
            Response = new Response(interaction.Response);
        }

        public Response Match(Request actualRequest)
        {
            if (Request.Match(actualRequest))
            {
                return Response;
            }
            return null;
        }
    }
}
