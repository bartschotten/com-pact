using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ComPact.MockConsumer;
using ComPact.ProviderTests.TestSupport;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComPact.ProviderTests
{
    [TestClass]
    public class ProviderTests
    {
        [TestMethod]
        public void ShouldVerifyPact()
        {
            var baseUrl = "http://localhost:9494";

            var mockConsumer = new MockConsumer.MockConsumer(baseUrl);

            var cts = new CancellationTokenSource();

            WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseUrls(baseUrl)
                .UseStartup<TestStartup>()
                .Build().RunAsync(cts.Token);

            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            mockConsumer.VerifyPact(pactDir + "recipe-consumer-recipe-service.json");

            cts.Cancel();
        }
    }
}
