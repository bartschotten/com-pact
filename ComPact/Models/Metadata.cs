using Newtonsoft.Json;

namespace ComPact.Models
{
    internal class Metadata
    {
        [JsonProperty("pactSpecification")]
        public PactSpecification PactSpecification { get; set; }
    }

    internal class PactSpecification
    {
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}

