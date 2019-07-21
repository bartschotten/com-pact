using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using ComPact.Models;

namespace ComPact.Builders
{
    public class RequestResponseMatcher: IRequestResponseMatcher
    {
        private readonly List<InteractionV2> _interactions;
        private readonly ILogger _logger;

        public RequestResponseMatcher(List<InteractionV2> interactions, ILogger logger)
        {
            _interactions = interactions;
            _logger = logger;
        }

        public Response FindMatch(Request actualRequest)
        {
            if (actualRequest == null)
            {
                throw new ArgumentNullException(nameof(actualRequest));
            }

            var matches = _interactions.Where(p => p.Request.Match(actualRequest));

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
                return matches.First().Response;
            }
        }
    }
}
