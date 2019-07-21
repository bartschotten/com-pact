using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ComPact.UnitTests
{
    [TestClass]
    public class PactWriterTests
    {
        [TestMethod]
        public void ShouldWritePactFile()
        {
            var requestHeaders = new Headers { { "Accept", "application/json" } };

            var pact = new PactV2
            {
                Consumer = new Pacticipant { Name = "consumer" },
                Provider = new Pacticipant { Name = "provider" },
                Interactions = new List<InteractionV2>
                {
                    new InteractionV2
                    {
                        Description = "TestInteraction",
                        Request = new Request
                        {
                            Method = Method.GET,
                            Path = "/",
                            Headers = requestHeaders
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Headers = new Dictionary<string, string>(),
                            Body = new
                            {
                                id = Guid.NewGuid()
                            }
                        }
                    }
                },
                Metadata = new Metadata { PactSpecification = new PactSpecification { Version = "3.0.0" } }
            };

            var config = new PactConfig();

            PactWriter.Write(pact, config);
        }
    }
}
