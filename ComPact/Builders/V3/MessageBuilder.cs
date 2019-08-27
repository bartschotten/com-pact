using ComPact.Models;
using ComPact.Models.V3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ComPact.Builders.V3
{
    public class MessageBuilder
    {
        private readonly Message _message = new Message();
        private bool _isVerified;

        public MessageBuilder Given(ProviderState providerState)
        {
            if (providerState is null)
            {
                throw new ArgumentNullException(nameof(providerState));
            }

            if (_message.ProviderStates == null)
            {
                _message.ProviderStates = new List<ProviderState>();
            }
            _message.ProviderStates.Add(providerState);
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
            CheckMessageHandler(messageHandler);

            try
            {
                var serializedContent = JsonConvert.SerializeObject(_message.Content);
                var content = JsonConvert.DeserializeObject<T>(serializedContent);
                InvokeMessageHandler(messageHandler, content);
            }
            catch
            {
                throw new PactException($"Could not deserialize the specified message content to {typeof(T)}.");
            }
            
            

            return this;
        }

        /// <summary>
        /// Invokes the provided message handler with the serialized message and checks that no exceptions are thrown.
        /// </summary>
        /// <param name="messageHandler">Actual handling code of your message consumer.</param>
        /// <returns></returns>
        public MessageBuilder VerifyConsumer(Action<string> messageHandler)
        {
            CheckMessageHandler(messageHandler);

            var serializedContent = JsonConvert.SerializeObject(_message.Content);
            InvokeMessageHandler(messageHandler, serializedContent);

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

        private void CheckMessageHandler<T>(Action<T> messageHandler)
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
        }

        private void InvokeMessageHandler<T>(Action<T> messageHandler, T content)
        {
            try
            {
                messageHandler.Invoke(content);
            }
            catch
            {
                throw new PactException("Message handler threw an exception");
            }
            _isVerified = true;
        }
    }
}
