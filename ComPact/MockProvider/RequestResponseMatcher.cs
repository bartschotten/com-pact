using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using System.Text;
using Newtonsoft.Json;
using ComPact.Models.V3;

namespace ComPact.MockProvider
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

            var request = new Request(httpRequest);

            var responses = _matchableInteractions.Select(m => m.Match(request)).Where(r => r != null);

            if (responses.Count() == 1)
            {
                httpResponseToReturn.StatusCode = responses.First().Status;
                foreach (var header in httpResponseToReturn.Headers)
                {
                    httpResponseToReturn.Headers.Add(header.Key, new StringValues((string)header.Value));
                }
                await httpResponseToReturn.Body.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responses.First().Body)));
            }
            else
            {
                var errorResponse = new RequestResponseMatchingErrorResponse()
                {
                    ActualRequest = request,
                    ExpectedRequests = _matchableInteractions.Select(m => m.Interaction.Request).ToList()
                };
                if (responses.Any())
                {
                    errorResponse.Message = "More than one matching response found for this request.";
                    responses.ToList().ForEach(r => _matchableInteractions.First(m => m.Interaction.Response == r).HasBeenMatched = false);
                }
                else
                {
                    errorResponse.Message = "No matching response set up for this request.";
                }
                httpResponseToReturn.StatusCode = 400;
                var stringToReturn = JsonConvert.SerializeObject(errorResponse);
                await httpResponseToReturn.Body.WriteAsync(Encoding.UTF8.GetBytes(stringToReturn));
            }
        }

        public bool AllHaveBeenMatched()
        {
            return _matchableInteractions.All(m => m.HasBeenMatched);
        }
    }
}
