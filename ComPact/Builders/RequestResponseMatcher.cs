using System.Collections.Generic;
using System.Linq;
using System;
using ComPact.Models;

namespace ComPact.Builders
{
    internal class RequestResponseMatcher: IRequestResponseMatcher
    {
        private readonly List<MatchableInteraction> _matchableInteractions;

        public RequestResponseMatcher(List<MatchableInteraction> interactions)
        {
            _matchableInteractions = interactions;
        }

        public ResponseV2 FindMatch(RequestV2 actualRequest)
        {
            if (actualRequest == null)
            {
                throw new ArgumentNullException(nameof(actualRequest));
            }

            var matches = _matchableInteractions.Where(m => m.Interaction.Request.Match(actualRequest));

            if (!matches.Any())
            {
                throw new PactException("No matching response set up for this request.");
            }
            else if (matches.Count() > 1)
            {
                throw new PactException("More than one matching response found for this request.");
            }
            else
            {
                matches.First().HasBeenMatched = true;
                return matches.First().Interaction.Response;
            }
        }

        public bool AllHaveBeenMatched()
        {
            return _matchableInteractions.All(m => m.HasBeenMatched);
        }
    }
}
