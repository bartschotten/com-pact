using ComPact.Exceptions;
using ComPact.MockProvider;
using ComPact.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComPact.Builders
{
    public abstract class PactBuilderBase
    {
        protected readonly string _consumer;
        protected readonly string _provider;
        protected readonly string _pactDir;
        protected readonly PactPublisher _pactPublisher;
        protected readonly CancellationTokenSource _cts;
        private readonly RequestResponseMatcher _matcher;
        internal MatchableInteractionList MatchableInteractions { get; private set; }

        internal PactBuilderBase(string consumer, string provider, string mockProviderServiceBaseUri, PactPublisher pactPublisher = null, string pactDir = null)
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
            MatchableInteractions = new MatchableInteractionList();

            _matcher = new RequestResponseMatcher(MatchableInteractions);

            ProviderWebHost.Run(mockProviderServiceBaseUri, _matcher, _cts);
        }

        internal void SetUp(MatchableInteraction matchableInteraction)
        {
            MatchableInteractions.AddUnique(matchableInteraction);
        }

        internal void ClearMatchableInteractions()
        {
            MatchableInteractions = new MatchableInteractionList();
        }

        internal async Task BuildAsync(IContract pact)
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

            pact.Consumer = new Pacticipant { Name = _consumer };
            pact.Provider = new Pacticipant { Name = _provider };

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
