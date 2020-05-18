using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Threading;

namespace ComPact.MockProvider
{
    internal static class ProviderWebHost
    {
        internal static void Run(string mockProviderServiceBaseUri, RequestResponseMatcher matcher, CancellationTokenSource cancellationTokenSource)
        {
            var host = WebHost.CreateDefaultBuilder()
                .UseUrls(mockProviderServiceBaseUri)
                .ConfigureKestrel(options => options.AllowSynchronousIO = true)
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        await matcher.MatchRequestAndReturnResponseAsync(context.Request, context.Response);
                    });
                })
                .Build();

            host.RunAsync(cancellationTokenSource.Token);
        }
    }
}
