using ComPact.ProviderTests.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.ProviderTests.TestSupport
{
    public class FakeRecipeRepository : IRecipeRepository
    {
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();

        public Recipe GetById(Guid id)
        {
            return Recipes.FirstOrDefault(r => r.Id == id);
        }
    }
}
