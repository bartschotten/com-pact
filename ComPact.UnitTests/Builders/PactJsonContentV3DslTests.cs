using ComPact.Builders.V3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ComPact.UnitTests.Builders
{
    [TestClass]
    public class PactJsonContentV3DslTests
    {
        [TestMethod]
        public void Integer()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Integer.Like(1).Named("number")).ToJToken();

            var expectedObject = new { number = 1 };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void Decimal()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Decimal.Like(1).Named("number")).ToJToken();

            var expectedObject = new { number = 1.0 };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }
    }
}
