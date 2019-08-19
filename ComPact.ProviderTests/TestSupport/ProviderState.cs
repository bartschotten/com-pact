using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComPact.ProviderTests.TestSupport
{
    public class ProviderState
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("params")]
        public Dictionary<string, string> Params { get; set; }
    }
}
