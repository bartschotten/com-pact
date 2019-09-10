using ComPact.Tests.Shared;
using System;

namespace ComPact.ProviderTests.TestSupport
{
    public class RecipeAddedProducer
    {
        private IRecipeRepository _recipeRepository;

        public RecipeAddedProducer(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        public RecipeAdded Send(string description)
        {
            return new RecipeAdded
            {
                EventId = Guid.NewGuid(),
                Recipe = _recipeRepository.GetLatestAdded()
            };
        }
    }
}
