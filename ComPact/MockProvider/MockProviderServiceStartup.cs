using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace ComPact.MockProvider
{
    internal class MockProviderServiceStartup
    {
        private readonly IRequestResponseMatcher _matcher;

        public MockProviderServiceStartup(IRequestResponseMatcher matcher)
        {
            _matcher = matcher;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await _matcher.MatchRequestAndReturnResponseAsync(context.Request, context.Response);
            });
        }
    }
}
