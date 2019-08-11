using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class RequestMatchTests
    {
        private readonly RequestV2 _expected;

        public RequestMatchTests()
        {
            _expected = new RequestV2
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = "skip=100&take=10",
                Body = "test"
            };
        }

        [TestMethod]
        public void ShouldMatchRequests()
        {
            var actual = new RequestV2
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json"} },
                Query = "skip=100&take=10",
                Body = "test"
            };

            Assert.IsTrue(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldMatchWhenActualHasExtraHeader()
        {
            var actual = new RequestV2
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" }, { "Host", "test" } },
                Query = "skip=100&take=10",
                Body = "test"
            };

            Assert.IsTrue(_expected.Match(actual));
            Assert.IsFalse(actual.Match(_expected));
        }

        [TestMethod]
        public void ShouldNotMatchWhenMethodDoesNotMatch()
        {
            var actual = new RequestV2
            {
                Method = Method.POST,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = "skip=100&take=10",
                Body = "test"
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenPathDoesNotMatch()
        {
            var actual = new RequestV2
            {
                Method = Method.GET,
                Path = "/test/resources",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = "skip=100&take=10",
                Body = "test"
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenHeaderDoesNotMatch()
        {
            var actual = new RequestV2
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/hal+json" } },
                Query = "skip=100&take=10",
                Body = "test"
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenQueryDoesNotMatch()
        {
            var actual = new RequestV2
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = "offset=100&limit=10",
                Body = "test"
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenBodyDoesNotMatch()
        {
            var actual = new RequestV2
            {
                Method = Method.GET,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = "skip=100&take=10",
                Body = new { }
            };

            Assert.IsFalse(_expected.Match(actual));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWhenNull()
        {
            new RequestV2().Match(null);
        }
    }
}
