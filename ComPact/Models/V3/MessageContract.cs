using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComPact.Models.V3
{
    internal class MessageContract
    {
        [JsonProperty("consumer")]
        internal Pacticipant Consumer { get; set; }
        [JsonProperty("provider")]
        internal Pacticipant Provider { get; set; }
        [JsonProperty("messages")]
        internal List<Message> Messages { get; set; }
        [JsonProperty("metadata")]
        internal Metadata Metadata { get; set; } = new Metadata { PactSpecification = new PactSpecification { Version = "3.0.0" } };
    }
}
