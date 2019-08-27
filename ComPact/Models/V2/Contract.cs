using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComPact.Models.V2
{
    internal class Contract: IContract
    {
        [JsonProperty("consumer")]
        public Pacticipant Consumer { get; set; }
        [JsonProperty("provider")]
        public Pacticipant Provider { get; set; }
        [JsonProperty("interactions")]
        internal List<Interaction> Interactions { get; set; }
        [JsonProperty("metadata")]
        internal Metadata Metadata { get; set; } = new Metadata { PactSpecification = new PactSpecification { Version = "2.0.0" } };
        public void SetEmptyValuesToNull()
        {
            Interactions.ForEach(i => i.SetEmptyValuesToNull());
        }
    }
}
