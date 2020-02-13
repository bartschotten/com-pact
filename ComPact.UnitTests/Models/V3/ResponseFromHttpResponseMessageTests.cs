using ComPact.Exceptions;
using ComPact.Models.V3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ComPact.UnitTests.Models.V3
{
    [TestClass]
    public class ResponseFromHttpResponseMessageTests
    {
        [TestMethod]
        public void ShouldCreateResponseFromHttpResponseMessage()
        {
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpResponseMessage.Headers.Add("Some-Header", "some value");
            httpResponseMessage.Content = new StringContent("\"text\"", Encoding.UTF8, "application/json");

            var response = new Response(httpResponseMessage);

            Assert.AreEqual(200, response.Status);
            Assert.IsTrue(response.Headers.Any(h => h.Key == "Some-Header" && h.Value == "some value"));
            Assert.IsTrue(response.Headers.Any(h => h.Key == "Content-Type" && h.Value == "application/json; charset=utf-8"));
            Assert.AreEqual("text", response.Body);
        }

        [TestMethod]
        public void ShouldCreateResponseFromHttpResponseMessageWithoutContent()
        {
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            httpResponseMessage.Headers.Add("Some-Header", "some value");

            var response = new Response(httpResponseMessage);

            Assert.AreEqual(404, response.Status);
            Assert.AreEqual(1, response.Headers.Count);
            Assert.IsTrue(response.Headers.Any(h => h.Key == "Some-Header" && h.Value == "some value"));
            Assert.IsNull(response.Body);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowWhenContentCannotBeDeserialized()
        {
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpResponseMessage.Content = new StringContent("text", Encoding.UTF8, "text/plain");

            try
            {
                var response = new Response(httpResponseMessage);
            }
            catch (PactException e)
            {
                Assert.AreEqual("Response body could not be deserialized to JSON. Content-Type was text/plain; charset=utf-8", e.Message);
                throw;
            }
        }
    }
}
