using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Models.V3
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
        internal Metadata Metadata { get; set; } = new Metadata { PactSpecification = new PactSpecification { Version = "3.0.0" } };

        internal Contract() { }
        internal Contract(V2.Contract contract)
        {
            Consumer = contract?.Consumer ?? throw new System.ArgumentNullException(nameof(contract));
            Provider = contract.Provider;
            Interactions = contract.Interactions.Select(i => new Interaction(i)).ToList();
        }
    }
}
