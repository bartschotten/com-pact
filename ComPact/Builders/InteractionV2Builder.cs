using ComPact.Models;
using System;

namespace ComPact.Builders
{
    public class InteractionV2Builder
    {
        private readonly InteractionV2 _interaction;

        public InteractionV2Builder()
        {
            _interaction = new InteractionV2();
        }

        public InteractionV2Builder Given(string providerState)
        {
            _interaction.ProviderState = providerState ?? throw new ArgumentNullException(nameof(providerState));
            return this;
        }

        public InteractionV2Builder UponReceiving(string description)
        {
            _interaction.Description = description ?? throw new ArgumentNullException(nameof(description));
            return this;
        }

        public InteractionV2Builder With(Request request)
        {
            _interaction.Request = request ?? throw new ArgumentNullException(nameof(request));
            return this;
        }

        public InteractionV2Builder WillRespondWith(Response response)
        {
            _interaction.Response = response ?? throw new ArgumentNullException(nameof(response));
            return this;
        }

        internal InteractionV2 Build()
        {
            return _interaction;
        }
    }
}
