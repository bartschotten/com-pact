using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ComPact.Tests.Shared
{
    public class Recipe
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ingredients")]
        public List<Ingredient> Ingredients { get; set; }
        [JsonProperty("instructions")]
        public string Instructions { get; set; }
    }
}
