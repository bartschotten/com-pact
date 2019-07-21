using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComPact.Models
{
    public class ProviderState
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("params")]
        public Dictionary<string, string> Params { get; set; }
    }
}
