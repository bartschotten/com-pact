using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComPact.ProviderTests.Domain
{
    public class Recipe
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        public string Instructions { get; set; }
    }
}
