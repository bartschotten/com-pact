using ComPact.Models;

namespace ComPact.Builders
{
    internal interface IRequestResponseMatcher
    {
        ResponseV2 FindMatch(RequestV2 actualRequest);
        bool AllHaveBeenMatched();
    }
}
