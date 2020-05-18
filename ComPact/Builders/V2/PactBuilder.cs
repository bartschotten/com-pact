using ComPact.MockProvider;
using ComPact.Models.V2;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ComPact.Builders.V2
{
    public class PactBuilder: PactBuilderBase
    {
        private List<Interaction> _interactions;

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
            : base(consumer, provider, mockProviderServiceBaseUri, pactPublisher, pactDir)
        {
            _interactions = new List<Interaction>();
        }

        /// <summary>
        /// Type Pact.Interaction...
        /// </summary>
        /// <param name="interactionBuilder"></param>
        public void SetUp(InteractionBuilder interactionBuilder)
        {
            var interaction = interactionBuilder.Build();
            base.SetUp(new MatchableInteraction(new Models.V3.Interaction(interaction)));
            _interactions.Add(interaction);
        }

        public void ClearInteractions()
        {
            _interactions = new List<Interaction>();
            ClearMatchableInteractions();
        }

        public async Task BuildAsync()
        {
            await base.BuildAsync(new Contract { Interactions = _interactions });
        }
    }
}
