using ComPact.Models;

namespace ComPact.Builders
{
    internal interface IRequestResponseMatcher
    {
        Response FindMatch(Request actualRequest);
        bool AllHaveBeenMatched();
    }
}
