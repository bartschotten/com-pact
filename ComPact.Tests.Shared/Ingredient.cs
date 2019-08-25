using Newtonsoft.Json;

namespace ComPact.Tests.Shared
{
    public class Ingredient
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("unit")]
        public string Unit { get; set; }
    }
}
