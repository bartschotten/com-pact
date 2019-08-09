using ComPact.Models;
using System;

namespace ComPact.Builders
{
    public class InteractionV2Builder
    {
        private readonly InteractionV2 _interaction = new InteractionV2();

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

        /// <summary>
        /// Type Pact.Request...
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public InteractionV2Builder With(RequestBuilder request)
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
        public InteractionV2Builder WillRespondWith(ResponseBuilder response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            _interaction.Response = response.Build();
            return this;
        }

        internal InteractionV2 Build()
        {
            return _interaction;
        }
    }
}
