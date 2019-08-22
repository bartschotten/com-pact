using ComPact.Tests.Shared;
using System;

namespace ComPact.ProviderTests
{
    public interface IRecipeRepository
    {
        Recipe GetById(Guid id);
    }
}
