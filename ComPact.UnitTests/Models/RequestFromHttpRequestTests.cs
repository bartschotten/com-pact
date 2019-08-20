using ComPact.Models;
using ComPact.Models.V3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class RequestFromHttpRequestTests
    {
        [TestMethod]
        public void ShouldCreatePactRequestFromHttpRequest()
        {
            var actualRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "GET",
                Path = "/test",
                Query = new QueryCollection(new Dictionary<string, StringValues> { { "skip", new StringValues("100") }, { "take", new StringValues("10") } }),
                Body = new MemoryStream()
            };
            actualRequest.Headers.Add("Accept", new StringValues("application/json"));

            var body = JsonConvert.SerializeObject("test");
            var streamWriter = new StreamWriter(actualRequest.Body, Encoding.UTF8);
            streamWriter.Write(body);
            streamWriter.Flush();
            actualRequest.Body.Seek(0, SeekOrigin.Begin);

            var pactRequest = new Request(actualRequest);

            Assert.AreEqual(Method.GET, pactRequest.Method);
            Assert.AreEqual("/test", pactRequest.Path);

            Assert.IsNotNull(pactRequest.Body);
            Assert.AreEqual("test", pactRequest.Body);

            Assert.IsNotNull(pactRequest.Headers);
            Assert.AreEqual(1, pactRequest.Headers.Count);
            Assert.AreEqual("application/json", pactRequest.Headers["Accept"]);

            Assert.AreEqual("skip=100&take=10", pactRequest.Query.ToQueryString());
        }

        [TestMethod]
        public void ShouldCreatePactRequestFromHttpRequestWithOnlyMethodAndPath()
        {
            var actualRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "GET",
                Path = "/test"
            };

            var pactRequest = new Request(actualRequest);

            Assert.AreEqual(Method.GET, pactRequest.Method);
            Assert.AreEqual("/test", pactRequest.Path);
            Assert.IsNull(pactRequest.Body);
            Assert.IsNotNull(pactRequest.Headers);
            Assert.AreEqual(0, pactRequest.Headers.Count);
            Assert.AreEqual(string.Empty, pactRequest.Query.ToQueryString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfRequestIsNull()
        {
            new Request(null as HttpRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowPactExceptionWhenMethodIsNotAllowed()
        {
            var actualRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "PATCH",
                Path = "/test"
            };

            try
            {
                var pactRequest = new Request(actualRequest);
            }
            catch (PactException e)
            {
                Assert.AreEqual("Received method PATCH is not allowed in Pact contracts.", e.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowPactExceptionWhenPathHasNoValue()
        {
            var actualRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "GET"
            };

            try
            {
                var pactRequest = new Request(actualRequest);
            }
            catch (PactException e)
            {
                Assert.AreEqual("Received path must have a value.", e.Message);
                throw;
            }
        }
    }
}
