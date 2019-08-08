using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComPact.UnitTests
{
    [TestClass]
    public class PactContentDslTests
    {
        [TestMethod]
        public void SimpleValue()
        {
            var pactJsonBody = PactJsonBody.With(Some.Element.Like("Hello world"));

            var expectedObject = "Hello world";

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void SimpleProperty()
        {
            var pactJsonBody = PactJsonBody.With(Some.Element.Named("greeting").Like("Hello world"));

            var expectedObject = new { greeting = "Hello world" };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void NestedObject()
        {
            var pactJsonBody = PactJsonBody.With
                (
                    Some.Object.Named("package").With
                    (
                        Some.Element.Like("Hello world").Named("greeting"), 
                        Some.Element.Like(1).Named("number")
                    )
                );

            var expectedObject = new { package = new { greeting = "Hello world", number = 1 } };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void RootLevelArray()
        {
            var pactJsonBody = PactJsonBody.With
                (
                    Some.Array.Of
                    (
                        Some.Element.Like("a"),
                        Some.Element.Like("b")
                    ).ContainingAtLeast(2)
                );

            var expectedObject = new[] { "a", "b" };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void RulesForSimpleValue()
        {
            var pactJsonElement = Some.Element.Like("Hello world");

            var matchingRules = new Dictionary<string, MatchingRule>();
            pactJsonElement.AddMatchingRules(matchingRules, "$");
        }
    }
}
