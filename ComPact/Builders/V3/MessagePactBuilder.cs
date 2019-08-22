using ComPact.Models;
using ComPact.Models.V3;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Builders.V3
{
    public class MessagePactBuilder
    {
        private readonly string _consumer;
        private readonly string _provider;
        private readonly List<Message> _messages;

        public MessagePactBuilder(string consumer, string provider)
        {
            _consumer = consumer ?? throw new System.ArgumentNullException(nameof(consumer));
            _provider = provider ?? throw new System.ArgumentNullException(nameof(provider));
            _messages = new List<Message>();
        }

        /// <summary>
        /// Type Pact.Message...
        /// </summary>
        /// <param name="message"></param>
        public MessagePactBuilder SetupMessage(MessageBuilder messageBuilder)
        {
            _messages.Add(messageBuilder.Build());
            return this;
        }

        public void Build()
        {
            if (!_messages.Any())
            {
                throw new PactException("Cannot build pact. No messages.");
            }

            var pact = new MessageContract
            {
                Consumer = new Pacticipant { Name = _consumer },
                Provider = new Pacticipant { Name = _provider },
                Messages = _messages
            };

            PactWriter.Write(pact);
        }
    }
}
