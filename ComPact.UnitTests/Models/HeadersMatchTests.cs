using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class HeadersMatchTests
    {
        [TestMethod]
        public void ShouldMatchForExactMatchingHeaders()
        {
            var actual = new Headers { { "Accept", "application/json" } };
            var expected = new Headers { { "Accept", "application/json" } };

            Assert.IsTrue(expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenKeysDoNotMatch()
        {
            var actual = new Headers { { "Accept", "application/json" } };
            var expected = new Headers { { "Accep", "application/json" } };

            Assert.IsFalse(expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotMatchWhenValuesDoNotMatch()
        {
            var actual = new Headers { { "Accept", "application/json" } };
            var expected = new Headers { { "Accept", "application/hal+json" } };

            Assert.IsFalse(expected.Match(actual));
        }

        [TestMethod]
        public void ShouldAllowForAdditionalActualHeader()
        {
            var actual = new Headers { { "Accept", "application/json" }, { "Host", "test" } };
            var expected = new Headers { { "Accept", "application/json" } };

            Assert.IsTrue(expected.Match(actual));
        }

        [TestMethod]
        public void ShouldNotAllowForAdditionalExpectedHeader()
        {
            var actual = new Headers { { "Accept", "application/json" } };
            var expected = new Headers { { "Accept", "application/json" }, { "Host", "test" } };

            Assert.IsFalse(expected.Match(actual));
        }

        [TestMethod]
        public void ShouldMatchWhenNoHeadersAreExpected()
        {
            var actual = new Headers();
            var expected = new Headers();

            Assert.IsTrue(expected.Match(actual));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWhenNull()
        {
            new Headers().Match(null);
        }
    }
}
