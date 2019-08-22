using ComPact.Tests.Shared;
using System;
using System.Collections.Generic;

namespace ComPact.ConsumerTests.Handler
{
    public class RecipeAddedHandler
    {
        public List<Recipe> ReceivedRecipes { get; set; } = new List<Recipe>();

        public void Handle(RecipeAdded recipeAdded)
        {
            if (recipeAdded is null)
            {
                throw new ArgumentNullException(nameof(recipeAdded));
            }

            ReceivedRecipes.Add(recipeAdded.Recipe);
        }
    }
}
