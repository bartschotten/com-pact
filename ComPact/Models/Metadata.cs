using Newtonsoft.Json;

namespace ComPact.Models
{
    internal class Metadata
    {
        [JsonProperty("pactSpecification")]
        public PactSpecification PactSpecification { get; set; }
        [JsonProperty("pact-specification")]
        public PactSpecification PactSpecificationWithDash { get; set; }
        [JsonProperty("pactSpecificationVersion")]
        public string PactSpecificationVersion { get; set; }

        public SpecificationVersion GetVersion()
        {
            var stringVersion = PactSpecification?.Version ?? PactSpecificationWithDash?.Version ?? PactSpecificationVersion;
            if (stringVersion.StartsWith("2"))
            {
                return SpecificationVersion.Two;
            }
            if (stringVersion.StartsWith("3"))
            {
                return SpecificationVersion.Three;
            }
            return SpecificationVersion.Unsupported;
        }
    }

    internal class PactSpecification
    {
        [JsonProperty("version")]
        public string Version { get; set; }
    }

    internal enum SpecificationVersion
    {
        Two,
        Three,
        Unsupported
    }
}

