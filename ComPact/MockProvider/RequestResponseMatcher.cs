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
    internal class RequestResponseMatcher
    {
        private readonly MatchableInteractionList _matchableInteractions;

        public RequestResponseMatcher(MatchableInteractionList interactions)
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

            string stringToReturn;
            if (response != null)
            {
                httpResponseToReturn.StatusCode = response.Status;
                foreach (var header in response.Headers)
                {
                    httpResponseToReturn.Headers.Add(header.Key, new StringValues((string)header.Value));
                }
                stringToReturn = JsonConvert.SerializeObject(response.Body);
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
                stringToReturn = JsonConvert.SerializeObject(errorResponse);
            }
            await httpResponseToReturn.Body.WriteAsync(Encoding.UTF8.GetBytes(stringToReturn), 0, Encoding.UTF8.GetByteCount(stringToReturn));
        }

        public bool AllHaveBeenMatched()
        {
            return _matchableInteractions.All(m => m.HasBeenMatched);
        }
    }
}
