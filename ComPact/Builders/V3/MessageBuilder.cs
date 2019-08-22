using ComPact.Models;
using ComPact.Models.V3;
using Newtonsoft.Json;
using System;

namespace ComPact.Builders.V3
{
    public class MessageBuilder
    {
        private readonly Message _message = new Message();
        private bool _isVerified;

        public MessageBuilder Given(ProviderState providerState)
        {
            _message.ProviderState = providerState ?? throw new ArgumentNullException(nameof(providerState));
            return this;
        }

        public MessageBuilder ShouldSend(string description)
        {
            _message.Description = description ?? throw new ArgumentNullException(nameof(description));
            return this;
        }

        /// <summary>
        /// Type Pact.JsonContent...
        /// </summary>
        /// <param name="messageContent"></param>
        /// <returns></returns>
        public MessageBuilder With(PactJsonContent messageContent)
        {
            _message.Content = messageContent.ToJToken();
            if (_message.MatchingRules == null)
            {
                _message.MatchingRules = new MatchingRuleCollection();
            }
            _message.MatchingRules.Body = messageContent.CreateV3MatchingRules();
            return this;
        }

        /// <summary>
        /// Invokes the provided message handler and checks that no exceptions are thrown.
        /// </summary>
        /// <typeparam name="T">Type to deserialize the defined message to.</typeparam>
        /// <param name="messageHandler">Actual handling code of your message consumer.</param>
        /// <returns></returns>
        public MessageBuilder VerifyConsumer<T>(Action<T> messageHandler)
        {
            if (messageHandler is null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            if (_message.Content == null)
            {
                throw new PactException("Message content has not been set. Please provide using the With method.");
            }

            _isVerified = false;
            try
            {
                var content = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(_message.Content));
                messageHandler.Invoke(content);
            }
            catch (InvalidCastException)
            {
                throw new PactException($"Could not deserialize the specified message content to {typeof(T)}.");
            }
            catch
            {
                throw new PactException("Message handler threw an exception");
            }
            _isVerified = true;

            return this;
        }

        internal Message Build()
        {
            if (!_isVerified)
            {
                throw new PactException("Consumer was not verified for message, please use the VerifyConsumer method.");
            }

            return _message;
        }
    }
}
