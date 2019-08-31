using ComPact.Builders;
using ComPact.Models;
using ComPact.Models.V2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ComPact.UnitTests.Builders
{
    [TestClass]
    public class PactWriterTests
    {
        [TestMethod]
        public void ShouldWritePactFile()
        {
            var pact = new Contract
            {
                Consumer = new Pacticipant { Name = "consumer" },
                Provider = new Pacticipant { Name = "provider" },
                Interactions = new List<Interaction>
                {
                    new Interaction
                    {
                        Description = "TestInteraction",
                        Request = new Request
                        {
                            Method = Method.GET,
                            Path = "/",
                            Headers = new Headers { { "Accept", "application/json" } }
                        },
                        Response = new Response
                        {
                            Status = 200,
                            Headers = new Headers { { "Content-Type", "application/json" } },
                            Body = new
                            {
                                id = Guid.NewGuid()
                            }
                        }
                    }
                },
                Metadata = new Metadata { PactSpecification = new PactSpecification { Version = "2.0.0" } }
            };

            PactWriter.Write(pact);
        }
    }
}
