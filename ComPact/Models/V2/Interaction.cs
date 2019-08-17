using Newtonsoft.Json;

namespace ComPact.Models.V2
{
    internal class Interaction
    {
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        [JsonProperty("providerState")]
        public string ProviderState { get; set; }
        [JsonProperty("request")]
        public Request Request { get; set; } = new Request();
        [JsonProperty("response")]
        public Response Response { get; set; } = new Response();

    }
}
