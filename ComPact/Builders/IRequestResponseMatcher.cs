using ComPact.Models;

namespace ComPact.Builders
{
    public interface IRequestResponseMatcher
    {
        Response FindMatch(Request actualRequest);
    }
}
