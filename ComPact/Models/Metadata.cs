using Newtonsoft.Json;

namespace ComPact.Models
{
    public class Metadata
    {
        [JsonProperty("pactSpecification")]
        public PactSpecification PactSpecification { get; set; }
    }

    public class PactSpecification
    {
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}

