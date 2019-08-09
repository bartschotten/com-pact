using ComPact.Builders;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComPact.UnitTests.Builders
{
    [TestClass]
    public class ResponseBodyBuilderTests
    {
        [TestMethod]
        public void SimpleValue()
        {
            var pactJsonBody = new ResponseBody().With(Some.Element.Like("Hello world")).ToJToken();

            var expectedObject = "Hello world";

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void SimpleProperty()
        {
            var pactJsonBody = new ResponseBody().With(Some.Element.Named("greeting").Like("Hello world")).ToJToken();

            var expectedObject = new { greeting = "Hello world" };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void NestedObject()
        {
            var pactJsonBody = new ResponseBody().With
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
            var pactJsonBody = new ResponseBody().With
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

        [TestMethod]
        public void RulesForSimpleValue()
        {
            var pactJsonElement = Some.Element.Like("Hello world");

            var matchingRules = new Dictionary<string, MatchingRule>();
            pactJsonElement.AddMatchingRules(matchingRules, "$");
        }
    }
}
