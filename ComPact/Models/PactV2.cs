using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComPact.Models
{
    public class PactV2
    {
        [JsonProperty("consumer")]
        public Pacticipant Consumer { get; set; }
        [JsonProperty("provider")]
        public Pacticipant Provider { get; set; }
        [JsonProperty("interactions")]
        public List<InteractionV2> Interactions { get; set; }
        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; } = new Metadata { PactSpecification = new PactSpecification { Version = "2.0.0" } };
    }
}
