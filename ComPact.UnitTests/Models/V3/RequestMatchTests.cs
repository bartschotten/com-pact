using ComPact.Models;
using ComPact.Models.V3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ComPact.UnitTests.Models.V3
{
    [TestClass]
    public class RequestMatchTests
    {
        private readonly Request _expected;

        public RequestMatchTests()
        {
            _expected = new Request
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = new Query("skip=100&take=10"),
                Body = "test"
            };
        }

        [TestMethod]
        public void ShouldMatchRequests()
        {
            var actual = new Request
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json"} },
                Query = new Query("skip=100&take=10"),
                Body = "test"
            };

            Assert.IsTrue(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldMatchWhenActualHasExtraHeader()
        {
            var actual = new Request
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" }, { "Host", "test" } },
                Query = new Query("skip=100&take=10"),
                Body = "test"
            };

            Assert.IsTrue(_expected.Match(actual));
            Assert.IsFalse(actual.Match(_expected));
        }

        [TestMethod]
        public void ShouldNotMatchWhenMethodDoesNotMatch()
        {
            var actual = new Request
            {
                Method = Method.POST,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = new Query("skip=100&take=10"),
                Body = "test"
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenPathDoesNotMatch()
        {
            var actual = new Request
            {
                Method = Method.GET,
                Path = "/test/resources",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = new Query("skip=100&take=10"),
                Body = "test"
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenHeaderDoesNotMatch()
        {
            var actual = new Request
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/hal+json" } },
                Query = new Query("skip=100&take=10"),
                Body = "test"
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenQueryDoesNotMatch()
        {
            var actual = new Request
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = new Query("offset=100&limit=10"),
                Body = "test"
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenBodyDoesNotMatch()
        {            
            var actual = new Request
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = new Query("skip=100&take=10"),
                Body = new { }
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWhenNull()
        {
            new Request().Match(null);
        }

        [TestMethod]
        public void ShouldMatchWhenBodyIsAnObject()
        {
            var expected = new Request
            {
                Method = Method.POST,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Body = new
                {
                    Text = "test",
                    Number = 120,
                    Collection = new Dictionary<string, string>() { { "key", "value" } }
                }
            };

            var actual = new Request
            {
                Method = Method.POST,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Body = new
                {
                    Text = "test",
                    Number = 120,
                    Collection = new Dictionary<string, string>() { { "key", "value" } }
                }
            };

            Assert.IsTrue(expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenBodyObjectIsDifferent()
        {
            var expected = new Request
            {
                Method = Method.POST,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Body = new
                {
                    Text = "test",
                    Number = 120,
                    Collection = new Dictionary<string, string>() { { "key", "otherValue" } }
                }
            };

            var actual = new Request
            {
                Method = Method.POST,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Body = new
                {
                    Text = "test",
                    Number = 120,
                    Collection = new Dictionary<string, string>() { { "key", "value" } }
                }
            };

            Assert.IsFalse(expected.Match(actual));
        }        
    }
}
