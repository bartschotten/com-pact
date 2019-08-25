using Newtonsoft.Json;
using System;

namespace ComPact.Tests.Shared
{
    public class RecipeAdded
    {
        [JsonProperty("eventId")]
        public Guid EventId { get; set; }
        [JsonProperty("recipe")]
        public Recipe Recipe { get; set; }
    }
}
