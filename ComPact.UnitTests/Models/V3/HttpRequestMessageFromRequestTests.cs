using ComPact.Models;
using ComPact.Models.V3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ComPact.UnitTests.Models.V3
{
    [TestClass]
    public class HttpRequestMessageFromRequestTests
    {
        [TestMethod]
        public void ShouldCreateHttpRequestMessageFromRequest()
        {
            var request = new Request
            {
                Method = Method.POST,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = new Query("skip=100&take=10"),
                Body = new
                {
                    text = "Hello World!"
                }
            };

            var httpRequestMessage = request.ToHttpRequestMessage();

            Assert.AreEqual(request.Method.ToString(), httpRequestMessage.Method.ToString());
            Assert.AreEqual(request.Path + "?" + request.Query.ToQueryString(), httpRequestMessage.RequestUri.ToString());
            Assert.AreEqual("application/json", httpRequestMessage.Headers.First().Value.First());
            Assert.AreEqual("{\"text\":\"Hello World!\"}", httpRequestMessage.Content.ReadAsStringAsync().Result);
        }

        [TestMethod]
        public void ShouldCreateHttpRequestMessageFromMinimalRequest()
        {
            var request = new Request
            {
                Method = Method.GET,
                Path = "/test",
            };

            var httpRequestMessage = request.ToHttpRequestMessage();

            Assert.AreEqual(request.Method.ToString(), httpRequestMessage.Method.ToString());
            Assert.AreEqual(request.Path, httpRequestMessage.RequestUri.ToString());
            Assert.IsFalse(httpRequestMessage.Headers.Any());
            Assert.IsNull(httpRequestMessage.Content);
        }
    }
}
