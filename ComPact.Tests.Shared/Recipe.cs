using System;
using System.Collections.Generic;

namespace ComPact.Tests.Shared
{
    public class Recipe
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public string Instructions { get; set; }
    }
}
