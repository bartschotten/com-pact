using ComPact.Models;
using ComPact.Models.V3;
using System;
using System.Collections.Generic;

namespace ComPact.Builders.V3
{
    public class InteractionBuilder
    {
        private readonly Interaction _interaction = new Interaction();

        public InteractionBuilder Given(ProviderState providerState)
        {
            if (providerState is null)
            {
                throw new ArgumentNullException(nameof(providerState));
            }

            if (_interaction.ProviderStates == null)
            {
                _interaction.ProviderStates = new List<ProviderState>();
            }
            _interaction.ProviderStates.Add(providerState);
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
