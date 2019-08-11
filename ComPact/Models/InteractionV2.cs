using Newtonsoft.Json;

namespace ComPact.Models
{
    internal class InteractionV2
    {
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        [JsonProperty("providerState")]
        public string ProviderState { get; set; }
        [JsonProperty("request")]
        public RequestV2 Request { get; set; } = new RequestV2();
        [JsonProperty("response")]
        public ResponseV2 Response { get; set; } = new ResponseV2();

    }
}
