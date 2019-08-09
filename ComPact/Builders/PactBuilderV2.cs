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
        private readonly string _consumer;
        private readonly string _provider;
        private readonly CancellationTokenSource _cts;
        private readonly ILogger _logger;
        private readonly IRequestResponseMatcher _matcher;
        private List<MatchableInteraction> _matchableInteractions;

        public PactBuilderV2(string consumer, string provider, string mockProviderServiceBaseUri, ILogger logger)
        {
            _cts = new CancellationTokenSource();
            _logger = logger;

            _consumer = consumer;
            _provider = provider;
            _matchableInteractions = new List<MatchableInteraction>();

            _matcher = new RequestResponseMatcher(_matchableInteractions, _logger);

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

        public void SetupInteraction(InteractionV2Builder interactionBuilder)
        {
            _matchableInteractions.Add(new MatchableInteraction(interactionBuilder.Build()));
        }

        public void ClearInteractions()
        {
            _matchableInteractions = new List<MatchableInteraction>();
        }

        public void Build()
        {
            _cts.Cancel();

            if (!_matchableInteractions.Any())
            {
                throw new PactException("Cannot build pact. No interactions.");
            }

            if (!_matcher.AllHaveBeenMatched())
            {
                throw new PactException("Cannot build pact. Not all mocked interactions have been called.");
            }

            var pact = new PactV2
            {
                Consumer = new Pacticipant { Name = _consumer },
                Provider = new Pacticipant { Name = _provider },
                Interactions = _matchableInteractions.Select(m => m.Interaction).ToList()
            };

            PactWriter.Write(pact, new PactConfig());
        }
    }
}
