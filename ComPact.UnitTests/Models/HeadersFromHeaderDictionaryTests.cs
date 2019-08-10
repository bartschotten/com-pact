using ComPact.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class HeadersFromHeaderDictionaryTests
    {
        [TestMethod]
        public void ShouldSupportMultipleHeaders()
        {
            var actualHeaders = new HeaderDictionary
            {
                { "Accept", new StringValues("application/json") },
                { "Host", new StringValues("test") }
            };

            var pactHeaders = new Headers(actualHeaders);

            Assert.AreEqual(actualHeaders.Count, pactHeaders.Count);
            Assert.AreEqual("application/json", pactHeaders["Accept"]);
            Assert.AreEqual("test", pactHeaders["Host"]);
        }

        [TestMethod]
        public void ShouldJoinMultipleValuesAsCommaSeparatedList()
        {
            var actualHeaders = new HeaderDictionary
            {
                { "Key", new StringValues(new string[] { "value1", "value2", "value3" }) }
            };

            var pactHeaders = new Headers(actualHeaders);

            Assert.AreEqual("value1,value2,value3", pactHeaders["Key"]);
        }

        [TestMethod]
        public void ShouldAllowForEmptyDictionary()
        {
            var actualHeaders = new HeaderDictionary();

            var pactHeaders = new Headers(actualHeaders);

            Assert.AreEqual(0, pactHeaders.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWhenNull()
        {
            new Headers(null as IHeaderDictionary);
        }
    }
}
