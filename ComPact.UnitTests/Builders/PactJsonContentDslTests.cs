using ComPact.Builders.V2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ComPact.UnitTests.Builders
{
    [TestClass]
    public class PactJsonContentDslTests
    {
        [TestMethod]
        public void SimpleValue()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Element.Like("Hello world")).ToJToken();

            var expectedObject = "Hello world";

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void SimpleProperty()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Element.Named("greeting").Like("Hello world")).ToJToken();

            var expectedObject = new { greeting = "Hello world" };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void NestedObject()
        {
            var pactJsonBody = Pact.JsonContent.With
                (
                    Some.Object.Named("package").With
                    (
                        Some.Element.Like("Hello world").Named("greeting"), 
                        Some.Element.Like(1).Named("number")
                    )
                ).ToJToken();

            var expectedObject = new { package = new { greeting = "Hello world", number = 1 } };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void RootLevelArray()
        {
            var pactJsonBody = Pact.JsonContent.With
                (
                    Some.Array.Of
                    (
                        Some.Element.Like("a"),
                        Some.Element.Like("b")
                    ).ContainingAtLeast(2)
                ).ToJToken();

            var expectedObject = new[] { "a", "b" };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }
    }
}
