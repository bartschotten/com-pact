using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComPact.Models.V3
{
    internal class MessageContract : IContract
    {
        [JsonProperty("consumer")]
        public Pacticipant Consumer { get; set; }
        [JsonProperty("provider")]
        public Pacticipant Provider { get; set; }
        [JsonProperty("messages")]
        internal List<Message> Messages { get; set; }
        [JsonProperty("metadata")]
        internal Metadata Metadata { get; set; } = new Metadata { PactSpecification = new PactSpecification { Version = "3.0.0" } };
        public void SetEmptyValuesToNull()
        {
            Messages.ForEach(i => i.SetEmptyValuesToNull());
        }
    }
}
