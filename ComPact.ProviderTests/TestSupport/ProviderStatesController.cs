using System;
using System.Collections.Generic;
using ComPact.Models;
using ComPact.ProviderTests.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ComPact.ProviderTests.TestSupport
{
    [Route("provider-states")]
    [ApiController]
    public class ProviderStatesontroller : ControllerBase
    {
        private readonly FakeRecipeRepository _recipeRepo;

        public ProviderStatesontroller(IRecipeRepository recipeRepo)
        {
            _recipeRepo = recipeRepo as FakeRecipeRepository;
        }

        [HttpPost]
        public ActionResult Post(ProviderState providerState)
        {
            if (providerState.Name.StartsWith("There is a recipe with id"))
            {
                var id = providerState.Name.Split('`')[1];

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

                _recipeRepo.Recipes.Add(recipe);
            }

            return Ok();
        }
    }
}
