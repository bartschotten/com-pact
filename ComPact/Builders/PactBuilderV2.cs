using ComPact.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ComPact.Builders
{
    public class PactBuilderV2
    {
        private readonly CancellationTokenSource _cts;
        private readonly ILogger _logger;

        public string Consumer { get; private set; }
        public string Provider { get; private set; }
        public List<InteractionV2> Interactions { get; private set; }

        public PactBuilderV2(string consumer, string provider, string mockProviderServiceBaseUri, ILogger logger)
        {
            _cts = new CancellationTokenSource();
            _logger = logger;

            Consumer = consumer;
            Provider = provider;
            Interactions = new List<InteractionV2>();

            var host = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseUrls(mockProviderServiceBaseUri)
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddSingleton<IRequestResponseMatcher>(new RequestResponseMatcher(Interactions, _logger));
                })
                .UseStartup<MockProviderServiceStartup>()
                .Build();

            host.RunAsync(_cts.Token);
        }

        public void SetupInteraction(InteractionV2 interaction)
        {
            Interactions.Add(interaction);
        }

        public void ClearInteractions()
        {
            Interactions = new List<InteractionV2>();
        }

        public void Build()
        {
            _cts.Cancel();

            var pact = new PactV2
            {
                Consumer = new Pacticipant { Name = Consumer },
                Provider = new Pacticipant { Name = Provider },
                Interactions = Interactions
            };

            PactWriter.Write(pact, new PactConfig());
        }
    }
}
