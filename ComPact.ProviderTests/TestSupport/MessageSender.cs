using ComPact.Models;
using ComPact.Tests.Shared;
using System;
using System.Collections.Generic;

namespace ComPact.ProviderTests.TestSupport
{
    public class MessageSender
    {
        public RecipeAdded Send(IEnumerable<ProviderState> providerStates, string description)
        {
            var recipeRepository = new FakeRecipeRepository();

            foreach (var providerState in providerStates)
            {
                if (providerState.Name.StartsWith("A new recipe has been added"))
                {
                    var id = providerState.Params["recipeId"];

                    var recipe = new Recipe
                    {
                        Id = Guid.Parse(id),
                        Name = "Pizza dough",
                        Instructions = "Mix the yeast with a little water and the sugar. Let it sit for 10 minutes. " +
                        "Add the flour, then add the salt and the oil and mix it all up. Then add the rest of the water. " +
                        "Knead for a good 10 or 15 minutes until the dough can be stetched and isn't too sticky to handle any more. " +
                        "Let it proof for about an hour covered with a tea towel or some plastic wrap.",
                        Ingredients = new List<Ingredient>
                    {
                        new Ingredient { Name = "Flour", Amount = 190, Unit = "gram" },
                        new Ingredient { Name = "Yeast", Amount = 5, Unit = "gram" },
                        new Ingredient { Name = "Sugar", Amount = 10, Unit = "gram" },
                        new Ingredient { Name = "Water", Amount = 120, Unit = "ml" },
                        new Ingredient { Name = "Olive oil", Amount = 10, Unit = "ml" },
                        new Ingredient { Name = "Salt", Amount = 5, Unit = "gram" }
                    }
                    };

                    recipeRepository.Add(recipe);
                }
            }

            return new RecipeAdded
            {
                EventId = Guid.NewGuid(),
                Recipe = recipeRepository.GetLatestAdded()
            };
        }
    }
}
