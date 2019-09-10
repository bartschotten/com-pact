using ComPact.Models;
using ComPact.Models.V3;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ComPact.Builders.V3
{
    public class MessagePactBuilder
    {
        private readonly string _consumer;
        private readonly string _provider;
        private readonly string _pactDir;
        private readonly PactPublisher _pactPublisher;
        private readonly List<Message> _messages;

        /// <summary>
        /// Generates a V3 message contract between a consumer and provider, 
        /// writes the contract to disk and optionally publishes to a Pact Broker using the supplied client.
        /// </summary>
        /// <param name="consumer">Name of consuming party of the contract.</param>
        /// <param name="provider">Name of the providing party of the contract.</param>
        /// <param name="pactPublisher">If not supplied the contract will not be published.</param>
        /// <param name="pactDir">Directory where the generated pact file will be written to. Defaults to the current project directory.</param>
        public MessagePactBuilder(string consumer, string provider, PactPublisher pactPublisher = null, string pactDir = null)
        {
            _consumer = consumer ?? throw new System.ArgumentNullException(nameof(consumer));
            _provider = provider ?? throw new System.ArgumentNullException(nameof(provider));

            _pactDir = pactDir;
            _pactPublisher = pactPublisher;

            _messages = new List<Message>();
        }

        /// <summary>
        /// Type Pact.Message...
        /// </summary>
        /// <param name="messageBuilder"></param>
        public MessagePactBuilder SetupMessage(MessageBuilder messageBuilder)
        {
            _messages.Add(messageBuilder.Build());
            return this;
        }

        public async Task BuildAsync()
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
