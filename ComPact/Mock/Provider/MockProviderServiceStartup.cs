﻿using ComPact.Models.V3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Text;

namespace ComPact.Mock.Provider
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
                await _matcher.MatchRequestAndReturnResponse(context.Request, context.Response);
            });
        }
    }
}
