using Newtonsoft.Json;

namespace ComPact.Models
{
    internal class ContractWithSomeVersion
    {
        [JsonProperty("metadata")]
        internal Metadata Metadata { get; set; }
    }
}
