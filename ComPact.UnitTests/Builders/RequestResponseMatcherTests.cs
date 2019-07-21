using ComPact.Builders;
using ComPact.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ComPact.UnitTests.Builders
{
    [TestClass]
    public class RequestResponseMatcherTests
    {
        [TestMethod]
        public void ShouldMatchSingleRequestToRespons()
        {
            var expectedRequest = new Request
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = "skip=100&take=10",
                Body = "test"
            };

            var expectedResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json"} },
                Body = "OK!"
            };

            var interactions = new List<InteractionV2> { new InteractionV2 { Request = expectedRequest, Response = expectedResponse } };

            var matcher = new RequestResponseMatcher(interactions, NullLogger.Instance);

            var actualResponse = matcher.FindMatch(expectedRequest);

            Assert.IsNotNull(actualResponse);
            Assert.AreEqual(200, actualResponse.Status);
            Assert.AreEqual("application/json", actualResponse.Headers["Content-Type"]);
            Assert.AreEqual("OK!", actualResponse.Body);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowPactExceptionIfNoMatchIsFound()
        {
            var matcher = new RequestResponseMatcher(new List<InteractionV2>(), NullLogger.Instance);

            try
            {
                var actualResponse = matcher.FindMatch(new Request());
            }
            catch (PactException e)
            {
                Assert.AreEqual("No matching response set up for this request.", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowWhenMoreThanOneRequestMatches()
        {
            var request1 = new Request
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = "skip=100&take=10",
                Body = "test"
            };

            var request2 = new Request
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = "skip=100&take=10",
                Body = "test"
            };

            var response1 = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = "OK!"
            };

            var response2 = new Response
            {
                Status = 404,
                Headers = new Headers(),
                Body = "Not OK!"
            };

            var interactions = new List<InteractionV2>
            {
                new InteractionV2 { Request = request1, Response = response1 },
                new InteractionV2 { Request = request2, Response = response2 }
            };

            var matcher = new RequestResponseMatcher(interactions, NullLogger.Instance);

            try
            {
                var actualResponse = matcher.FindMatch(request1);
            }
            catch (PactException e)
            {
                Assert.AreEqual("More than one matching response found for this request.", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWhenNull()
        {
            var matcher = new RequestResponseMatcher(new List<InteractionV2>(), NullLogger.Instance);

            matcher.FindMatch(null);
        }
    }
}
