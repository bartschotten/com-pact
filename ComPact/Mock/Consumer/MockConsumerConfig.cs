using ComPact.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ComPact.Mock.Consumer
{
    public class MockConsumerConfig
    {
        /// <summary>
        /// The base url where to call your actual provider service. To set up provider states, {base-url}/provider-states will be called.
        /// </summary>
        public string ProviderBaseUrl { get; set; }
        /// <summary>
        /// For message interactions, this function will be called with the provider states and decription defined in the contract as parameters.
        /// It should return the message that your code produces, so the mock consumer can verify it.
        /// If a string is returned, the mock consumer will assume that it is a serialized json string and try to deserialize it.
        /// </summary>
        public Func<IEnumerable<ProviderState>, string, object> MessageProducer { get; set; }
        /// <summary>
        /// Client that can be used to connect to your Pact Broker to retrieve pacts and (optionally) publish verification results. 
        /// Should be set up with the correct base URL and if needed any necessary headers.
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
