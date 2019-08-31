using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using System.Text;
using Newtonsoft.Json;

namespace ComPact.Mock.Provider
{
    internal class RequestResponseMatcher: IRequestResponseMatcher
    {
        private readonly List<MatchableInteraction> _matchableInteractions;

        public RequestResponseMatcher(List<MatchableInteraction> interactions)
        {
            _matchableInteractions = interactions;
        }

        public async Task MatchRequestAndReturnResponseAsync(HttpRequest httpRequest, HttpResponse httpResponseToReturn)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            var request = new Models.V3.Request(httpRequest);

            var responses = _matchableInteractions.Select(m => m.Match(request));

            if (!responses.Any())
            {
                throw new PactException("No matching response set up for this request.");
            }
            else if (responses.Count() > 1)
            {
                throw new PactException("More than one matching response found for this request.");
            }
            else
            {
                httpResponseToReturn.StatusCode = responses.First().Status;
                foreach (var header in httpResponseToReturn.Headers)
                {
                    httpResponseToReturn.Headers.Add(header.Key, new StringValues((string)header.Value));
                }
                await httpResponseToReturn.Body.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responses.First().Body)));
            }
        }

        public bool AllHaveBeenMatched()
        {
            return _matchableInteractions.All(m => m.HasBeenMatched);
        }
    }
}
