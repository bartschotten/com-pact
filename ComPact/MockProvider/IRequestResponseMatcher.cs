using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ComPact.MockProvider
{
    internal interface IRequestResponseMatcher
    {
        Task MatchRequestAndReturnResponseAsync(HttpRequest httpRequest, HttpResponse httpResponseToReturn);
        bool AllHaveBeenMatched();
    }
}
