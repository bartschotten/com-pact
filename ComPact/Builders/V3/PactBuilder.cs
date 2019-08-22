using ComPact.Mock.Provider;
using ComPact.Models;
using ComPact.Models.V3;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ComPact.Builders.V3
{
    public class PactBuilder
    {
        private readonly string _consumer;
        private readonly string _provider;
        private readonly CancellationTokenSource _cts;
        private readonly IRequestResponseMatcher _matcher;
        private List<MatchableInteraction> _matchableInteractions;

        public PactBuilder(string consumer, string provider, string mockProviderServiceBaseUri)
        {
            if (mockProviderServiceBaseUri is null)
            {
                throw new System.ArgumentNullException(nameof(mockProviderServiceBaseUri));
            }

            _cts = new CancellationTokenSource();

            _consumer = consumer ?? throw new System.ArgumentNullException(nameof(consumer));
            _provider = provider ?? throw new System.ArgumentNullException(nameof(provider));
            _matchableInteractions = new List<MatchableInteraction>();

            _matcher = new RequestResponseMatcher(_matchableInteractions);

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

        public void SetupInteraction(InteractionBuilder interactionBuilder)
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

            var pact = new Contract
            {
                Consumer = new Pacticipant { Name = _consumer },
                Provider = new Pacticipant { Name = _provider },
                Interactions = _matchableInteractions.Select(m => m.Interaction as Interaction).ToList()
            };

            PactWriter.Write(pact);
        }
    }
}
