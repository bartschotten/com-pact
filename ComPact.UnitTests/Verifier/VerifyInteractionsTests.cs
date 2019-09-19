using ComPact.Exceptions;
using ComPact.Models.V3;
using ComPact.Verifier;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ComPact.UnitTests.Verifier
{
    [TestClass]
    public class VerifyInteractionsTests
    {
        private List<Interaction> _interactions;

        public VerifyInteractionsTests()
        {
            _interactions = new List<Interaction>
            {
                new Interaction
                {
                    Description = "An interaction",
                    ProviderStates = new List<ComPact.Models.ProviderState> {new ComPact.Models.ProviderState { Name = "Some state"} },
                    Request = new Request
                    {
                        Method = ComPact.Models.Method.POST
                    },
                    Response = new Response
                    {
                        Status = 200
                    }
                },
                new Interaction
                {
                    Description = "Another interaction",
                    ProviderStates = new List<ComPact.Models.ProviderState> {new ComPact.Models.ProviderState { Name = "Some state"} },
                    Request = new Request
                    {
                        Method = ComPact.Models.Method.PUT
                    },
                    Response = new Response
                    {
                        Status = 200
                    }
                }
            };
        }

        [TestMethod]
        public void ShouldReturnSuccessfulTest()
        {
            var tests = PactVerifier.VerifyInteractions(_interactions, (req) => new RestResponse { StatusCode = HttpStatusCode.OK }, (p) => { });

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("passed", tests.First().Status);
            Assert.AreEqual("An interaction", tests.First().Description);
        }

        [TestMethod]
        public void ShouldReturnFailedTestWhenResponsesDoNotMatch()
        {
            var tests = PactVerifier.VerifyInteractions(_interactions, (req) => new RestResponse { StatusCode = HttpStatusCode.NotFound }, (p) => { });

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("failed", tests.First().Status);
            Assert.AreEqual("An interaction", tests.First().Description);
        }

        [TestMethod]
        public void ShouldReturnFailedTestWhenHandlerThrowsPactVerificationException()
        {
            var tests = PactVerifier.VerifyInteractions(_interactions, (req) => new RestResponse { StatusCode = HttpStatusCode.OK }, 
                (p) => throw new PactVerificationException("Unknown provider state."));

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("failed", tests.First().Status);
            Assert.AreEqual("An interaction", tests.First().Description);
            Assert.IsTrue(tests.First().Issues.First().Contains("Unknown provider state."));
        }
    }
}
