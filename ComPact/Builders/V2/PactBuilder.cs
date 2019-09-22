using ComPact.Exceptions;
using ComPact.MockProvider;
using ComPact.Models;
using ComPact.Models.V2;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComPact.Builders.V2
{
    public class PactBuilder
    {
        private readonly string _consumer;
        private readonly string _provider;
        private readonly string _pactDir;
        private readonly PactPublisher _pactPublisher;
        private readonly CancellationTokenSource _cts;
        private readonly RequestResponseMatcher _matcher;
        private List<Interaction> _interactions;
        private MatchableInteractionList _matchableInteractions;

        /// <summary>
        /// Sets up a mock provider service, generates a V2 contract between a consumer and provider, 
        /// writes the contract to disk and optionally publishes to a Pact Broker using the supplied client.
        /// </summary>
        /// <param name="consumer">Name of consuming party of the contract.</param>
        /// <param name="provider">Name of the providing party of the contract.</param>
        /// <param name="mockProviderServiceBaseUri">URL where you will call the mock provider service to verify your consumer.</param>
        /// <param name="pactPublisher">If not supplied the contract will not be published.</param>
        /// <param name="pactDir">Directory where the generated pact file will be written to. Defaults to the current project directory.</param>
        public PactBuilder(string consumer, string provider, string mockProviderServiceBaseUri, PactPublisher pactPublisher = null, string pactDir = null)
        {
            if (mockProviderServiceBaseUri is null)
            {
                throw new System.ArgumentNullException(nameof(mockProviderServiceBaseUri));
            }

            _pactDir = pactDir;
            _pactPublisher = pactPublisher;

            _cts = new CancellationTokenSource();

            _consumer = consumer ?? throw new System.ArgumentNullException(nameof(consumer));
            _provider = provider ?? throw new System.ArgumentNullException(nameof(provider));
            _interactions = new List<Interaction>();
            _matchableInteractions = new MatchableInteractionList();

            _matcher = new RequestResponseMatcher(_matchableInteractions);

            ProviderWebHost.Run(mockProviderServiceBaseUri, _matcher, _cts);
        }

        /// <summary>
        /// Type Pact.Interaction...
        /// </summary>
        /// <param name="interactionBuilder"></param>
        public void SetUp(InteractionBuilder interactionBuilder)
        {
            var interaction = interactionBuilder.Build();
            _matchableInteractions.AddUnique(new MatchableInteraction(new Models.V3.Interaction(interaction)));
            _interactions.Add(interaction);
        }

        public void ClearInteractions()
        {
            _interactions = new List<Interaction>();
            _matchableInteractions = new MatchableInteractionList();
        }

        public async Task BuildAsync()
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
                Interactions = _interactions
            };

            if (_pactDir != null)
            {
                PactWriter.Write(pact, _pactDir);
            }
            else
            {
                PactWriter.Write(pact);
            }

            if (_pactPublisher != null)
            {
                await _pactPublisher.PublishAsync(pact);
            }
        }
    }
}
