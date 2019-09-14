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

            var response = _matchableInteractions.Select(m => m.Match(request)).Where(r => r != null).LastOrDefault();

            if (response != null)
            {
                httpResponseToReturn.StatusCode = response.Status;
                foreach (var header in response.Headers)
                {
                    httpResponseToReturn.Headers.Add(header.Key, new StringValues((string)header.Value));
                }
                await httpResponseToReturn.Body.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response.Body)));
            }
            else
            {
                var errorResponse = new RequestResponseMatchingErrorResponse()
                {
                    ActualRequest = request,
                    ExpectedRequests = _matchableInteractions.Select(m => m.Interaction.Request).ToList(),
                    Message = "No matching response set up for this request."
                };
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
