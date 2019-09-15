using ComPact.Builders.V2;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComPact.UnitTests.Builders
{
    [TestClass]
    public class CreateV2MatchingRulesTests
    {
        [TestMethod]
        public void SimpleValue()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Element.Like("Hello world"));

            var matchingRules = pactJsonBody.CreateV2MatchingRules();

            Assert.AreEqual(MatcherType.type, matchingRules["$.body"].MatcherType);
        }

        [TestMethod]
        public void NamedValueInObject()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Element.Like("Hello world").Named("greeting"));

            var matchingRules = pactJsonBody.CreateV2MatchingRules();

            Assert.AreEqual(MatcherType.type, matchingRules["$.body.greeting"].MatcherType);
        }

        [TestMethod]
        public void ExactValue()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Element.WithTheExactValue("Hello world"));

            var matchingRules = pactJsonBody.CreateV2MatchingRules();

            Assert.IsNull(matchingRules);
        }

        [TestMethod]
        public void ElementWithinArray()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Array.Named("anArray").Of(Some.Element.Like("Hello world")));

            var matchingRules = pactJsonBody.CreateV2MatchingRules();

            Assert.AreEqual(MatcherType.type, matchingRules["$.body.anArray[0]"].MatcherType);
        }

        [TestMethod]
        public void ElementWithinArrayWithMin()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Array.Named("anArray").ContainingAtLeast(2).Of(Some.Element.Like("Hello world")));

            var matchingRules = pactJsonBody.CreateV2MatchingRules();

            Assert.AreEqual(2, matchingRules.Count);
            Assert.AreEqual(MatcherType.type, matchingRules["$.body.anArray"].MatcherType);
            Assert.AreEqual(2, matchingRules["$.body.anArray"].Min);
            Assert.AreEqual(MatcherType.type, matchingRules["$.body.anArray[0]"].MatcherType);
        }

        [TestMethod]
        public void Regex()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.String.LikeRegex("Hello world", "Hello.*"));

            var matchingRules = pactJsonBody.CreateV2MatchingRules();

            Assert.AreEqual(MatcherType.regex, matchingRules["$.body"].MatcherType);
            Assert.AreEqual("Hello.*", matchingRules["$.body"].Regex);
        }

        [TestMethod]
        public void ArrayWithStar()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Array.Named("anArray").InWhichEveryElementIs(Some.Element.Like("Hello world")));

            var matchingRules = pactJsonBody.CreateV2MatchingRules();

            Assert.AreEqual(2, matchingRules.Count);
            Assert.AreEqual(MatcherType.type, matchingRules["$.body.anArray[*]"].MatcherType);
            Assert.AreEqual(1, matchingRules["$.body.anArray"].Min);
        }

        [TestMethod]
        public void SimpleString()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.String.Like("Hello world").Named("greeting"));

            var matchingRules = pactJsonBody.CreateV2MatchingRules();

            Assert.AreEqual(MatcherType.type, matchingRules["$.body.greeting"].MatcherType);
        }
    }
}
