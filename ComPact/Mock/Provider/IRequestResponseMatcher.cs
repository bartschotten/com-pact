using ComPact.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ComPact.Mock.Provider
{
    internal interface IRequestResponseMatcher
    {
        Task MatchRequestAndReturnResponseAsync(HttpRequest httpRequest, HttpResponse httpResponseToReturn);
        bool AllHaveBeenMatched();
    }
}
