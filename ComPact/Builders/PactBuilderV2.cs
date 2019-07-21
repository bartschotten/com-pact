using ComPact.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ComPact.Builders
{
    public class PactBuilderV2
    {
        private readonly CancellationTokenSource _cts;
        private readonly ILogger _logger;
        private readonly IRequestResponseMatcher _matcher;

        public string Consumer { get; private set; }
        public string Provider { get; private set; }
        public List<MatchableInteraction> MatchableInteractions { get; private set; }

        public PactBuilderV2(string consumer, string provider, string mockProviderServiceBaseUri, ILogger logger)
        {
            _cts = new CancellationTokenSource();
            _logger = logger;

            Consumer = consumer;
            Provider = provider;
            MatchableInteractions = new List<MatchableInteraction>();

            _matcher = new RequestResponseMatcher(MatchableInteractions, _logger);

            var host = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseUrls(mockProviderServiceBaseUri)
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddSingleton(_matcher);
                })
                .UseStartup<MockProviderServiceStartup>()
                .Build();

            host.RunAsync(_cts.Token);
        }

        public void SetupInteraction(InteractionV2 interaction)
        {
            MatchableInteractions.Add(new MatchableInteraction(interaction));
        }

        public void ClearInteractions()
        {
            MatchableInteractions = new List<MatchableInteraction>();
        }

        public void Build()
        {
            _cts.Cancel();

            if (!MatchableInteractions.Any())
            {
                throw new PactException("Cannot build pact. No interactions.");
            }

            if (!_matcher.AllHaveBeenMatched())
            {
                throw new PactException("Cannot build pact. Not all mocked interactions have been called.");
            }

            var pact = new PactV2
            {
                Consumer = new Pacticipant { Name = Consumer },
                Provider = new Pacticipant { Name = Provider },
                Interactions = MatchableInteractions.Select(m => m.Interaction).ToList()
            };

            PactWriter.Write(pact, new PactConfig());
        }
    }
}
