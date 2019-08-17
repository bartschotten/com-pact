using ComPact.Models.V2;
using System;

namespace ComPact.Builders.V2
{
    public class InteractionBuilder
    {
        private readonly Interaction _interaction = new Interaction();

        public InteractionBuilder Given(string providerState)
        {
            _interaction.ProviderState = providerState ?? throw new ArgumentNullException(nameof(providerState));
            return this;
        }

        public InteractionBuilder UponReceiving(string description)
        {
            _interaction.Description = description ?? throw new ArgumentNullException(nameof(description));
            return this;
        }

        /// <summary>
        /// Type Pact.Request...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public InteractionBuilder With(RequestBuilder request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _interaction.Request = request.Build();
            return this;
        }

        /// <summary>
        /// Type Pact.Response...
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public InteractionBuilder WillRespondWith(ResponseBuilder response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            _interaction.Response = response.Build();
            return this;
        }

        internal Interaction Build()
        {
            return _interaction;
        }
    }
}
