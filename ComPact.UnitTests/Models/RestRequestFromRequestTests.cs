using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class RestRequestFromRequestTests
    {
        [TestMethod]
        public void ShouldCreateRestRequestFromRequest()
        {
            var request = new Request
            {
                Method = Method.POST,
                Path = "/test",
                Headers = new Headers { { "Accept", "application/json" } },
                Query = "skip=100&take=10",
                Body = new
                {
                    text = "Hello World!"
                }
            };

            var restRequest = request.ToRestRequest();

            Assert.AreEqual(request.Method.ToString(), restRequest.Method.ToString());
            Assert.AreEqual(request.Path, restRequest.Resource);
            Assert.AreEqual(request.Query, string.Join("&", restRequest.Parameters.Where(p => p.Type == RestSharp.ParameterType.QueryStringWithoutEncode)
                .Select(p => p.Name + "=" + p.Value)));
            Assert.AreEqual("application/json", restRequest.Parameters.First(p => p.Type == RestSharp.ParameterType.HttpHeader).Value);
            Assert.AreEqual("{\"text\":\"Hello World!\"}", JsonConvert.SerializeObject(restRequest.Parameters.First(p => p.Type == RestSharp.ParameterType.RequestBody).Value));

        }
    }
}
