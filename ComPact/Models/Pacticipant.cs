using Newtonsoft.Json;

namespace ComPact.Models
{
    public class Pacticipant
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
