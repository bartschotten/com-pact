using ComPact.Tests.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.ProviderTests.TestSupport
{
    public class FakeRecipeRepository : IRecipeRepository
    {
        public List<Recipe> Recipes { get; set; } = new List<Recipe>();

        public void Add(Recipe recipe)
        {
            Recipes.Add(recipe);
        }

        public Recipe GetById(Guid id)
        {
            return Recipes.FirstOrDefault(r => r.Id == id);
        }

        public Recipe GetLatestAdded()
        {
            return Recipes.LastOrDefault();
        }
    }
}
