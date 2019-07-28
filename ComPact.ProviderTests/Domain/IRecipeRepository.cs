using System;

namespace ComPact.ProviderTests.Domain
{
    public interface IRecipeRepository
    {
        Recipe GetById(Guid id);
    }
}
