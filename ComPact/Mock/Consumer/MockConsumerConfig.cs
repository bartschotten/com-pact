using ComPact.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ComPact.Mock.Consumer
{
    public class MockConsumerConfig
    {
        /// <summary>
        /// The base url where to call your actual provider service. To set up provider states, <base-url>/provider-states will be called.
        /// </summary>
        public string ProviderBaseUrl { get; set; }
        /// <summary>
        /// An action that will be invoked for the providerStates of a message interaction.
        /// </summary>
        public Action<IEnumerable<ProviderState>> MessageProviderStateHandler { get; set; }
        /// <summary>
        /// A function that will be called to retrieve the actual message you produce.
        /// </summary>
        public Func<object> MessageProducer { get; set; }
        /// <summary>
        /// Client that can be used to connect to your Pact Broker to retrieve pacts and (optionally) publish verification results.
        /// </summary>
        public HttpClient PactBrokerClient { get; set; }
        /// <summary>
        /// Whether to actually publish the verification results.
        /// </summary>
        public bool PublishVerificationResults { get; set; }
        /// <summary>
        /// The provider version to use when publishing verification results.
        /// </summary>
        public string ProviderVersion { get; set; }
    }
}
