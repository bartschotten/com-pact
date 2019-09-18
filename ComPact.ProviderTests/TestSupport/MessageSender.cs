using ComPact.Models;
using ComPact.Tests.Shared;
using System;

namespace ComPact.ProviderTests.TestSupport
{
    public class MessageSender
    {
        public FakeRecipeRepository RecipeRepository { get; set; }

        public MessageSender(FakeRecipeRepository recipeRepository)
        {
            RecipeRepository = recipeRepository;
        }

        public RecipeAdded Send(string description)
        {
            return new RecipeAdded
            {
                EventId = Guid.NewGuid(),
                Recipe = RecipeRepository.GetLatestAdded()
            };
        }
    }
}
