using ComPact.Matchers;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class ResponseMatchTests
    {
        [TestMethod]
        public void ShouldReturnNoDifferencesForIdenticalResponses()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = new
                {
                    name = "Hello world!"
                }
            };

            var differences = response.Match(response);

            Assert.AreEqual(0, differences.Count);
        }

        [TestMethod]
        public void ShouldReturnDifferencesWhenValueDoesNotMatch()
        {
            var expectedResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = new
                {
                    name = "Hello world!"
                }
            };

            var actualResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = new
                {
                    name = "Hello Pact!"
                }
            };

            var differences = expectedResponse.Match(actualResponse);

            Assert.AreEqual(1, differences.Count);
            Assert.AreEqual("Expected Hello world! at body.name, but was Hello Pact!", differences.First());
        }

        [TestMethod]
        public void ShouldReturnDifferencesWhenPropertyCannotBeFound()
        {
            var expectedResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = new
                {
                    name = "Hello world!"
                }
            };

            var actualResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = new
                {
                    text = "Hello world!"
                }
            };

            var differences = expectedResponse.Match(actualResponse);

            Assert.AreEqual(1, differences.Count);
            Assert.AreEqual("Property body.name was not present in the actual response", differences.First());
        }

        [TestMethod]
        public void ShouldTakeTypeMatchingRuleIntoAccount()
        {
            var expectedResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = new
                {
                    name = Match.Type("Hello world!")
                }
            };

            expectedResponse = expectedResponse.ConvertMatchingRules();

            var actualResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = new
                {
                    name = "Hello Pact!"
                }
            };

            var differences = expectedResponse.Match(actualResponse);

            Assert.AreEqual(0, differences.Count);
        }

        [TestMethod]
        public void ShouldNoteWhenTypeDoesNotMatch()
        {
            var expectedResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = new
                {
                    name = Match.Type("Hello world!")
                }
            };

            expectedResponse = expectedResponse.ConvertMatchingRules();

            var actualResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = new
                {
                    name = 1
                }
            };

            var differences = expectedResponse.Match(actualResponse);

            Assert.AreEqual(1, differences.Count);
            Assert.AreEqual("Expected value of type String (like \"Hello world!\") at body.name, but was 1 (Integer)", differences.First());
        }
    }
}
