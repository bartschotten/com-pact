using ComPact.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.MockProvider
{
    internal class MatchableInteractionList : List<MatchableInteraction>
    {
        internal void AddUnique(MatchableInteraction matchableInteraction)
        {
            if (this.All(m => m.Interaction.ProviderStatesAndRequestCanBeDistinguishedFrom(matchableInteraction.Interaction)))
            {
                Add(matchableInteraction);
            }
            else
            {
                throw new PactException("Cannot add multiple interactions with the same provider states and requests. The provider will not be able to distinguish between them.");
            }
        }
    }
}
