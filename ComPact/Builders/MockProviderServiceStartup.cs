using ComPact.Builders;
using ComPact.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Text;

namespace ComPact
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
                var request = new Request(context.Request);
                var response = _matcher.FindMatch(request);

                context.Response.StatusCode = response.Status;
                foreach(var header in response.Headers)
                {
                    context.Response.Headers.Add(header.Key, new Microsoft.Extensions.Primitives.StringValues(header.Value));
                }
                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response.Body)));
            });
        }
    }
}
