using ComPact.Models.V2;

namespace ComPact.Mock.Provider
{
    internal interface IRequestResponseMatcher
    {
        Response FindMatch(Request actualRequest);
        bool AllHaveBeenMatched();
    }
}
