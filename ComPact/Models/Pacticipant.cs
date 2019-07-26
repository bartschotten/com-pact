using Newtonsoft.Json;

namespace ComPact.Models
{
    internal class Pacticipant
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
