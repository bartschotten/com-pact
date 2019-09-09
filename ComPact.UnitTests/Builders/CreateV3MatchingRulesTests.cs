using ComPact.Builders.V3;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ComPact.UnitTests.Builders
{
    [TestClass]
    public class CreateV3MatchingRulesTests
    {
        [TestMethod]
        public void SimpleValue()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Element.Like("Hello world"));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(MatcherType.type, matchingRules["$"].Matchers.First().MatcherType);
        }

        [TestMethod]
        public void NamedValueInObject()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Element.Like("Hello world").Named("greeting"));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(MatcherType.type, matchingRules["$.greeting"].Matchers.First().MatcherType);
        }

        [TestMethod]
        public void ExactValue()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Element.WithTheExactValue("Hello world"));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(MatcherType.equality, matchingRules["$"].Matchers.First().MatcherType);
        }

        [TestMethod]
        public void ElementWithinArray()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Array.Named("anArray").Of(Some.Element.Like("Hello world")));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(MatcherType.type, matchingRules["$.anArray[0]"].Matchers.First().MatcherType);
        }

        [TestMethod]
        public void ElementWithinArrayWithMin()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Array.Named("anArray").ContainingAtLeast(2).Of(Some.Element.Like("Hello world")));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(2, matchingRules.Count);
            Assert.AreEqual(MatcherType.type, matchingRules["$.anArray"].Matchers.First().MatcherType);
            Assert.AreEqual(2, matchingRules["$.anArray"].Matchers.First().Min);
            Assert.AreEqual(MatcherType.type, matchingRules["$.anArray[0]"].Matchers.First().MatcherType);
        }

        [TestMethod]
        public void Regex()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.String.Like("Hello world", "Hello.*"));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(MatcherType.regex, matchingRules["$"].Matchers.First().MatcherType);
            Assert.AreEqual("Hello.*", matchingRules["$"].Matchers.First().Regex);
        }

        [TestMethod]
        public void ArrayWithStar()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Array.Named("anArray").InWhichEveryElementIsLike(Some.Element.Like("Hello world")));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(2, matchingRules.Count);
            Assert.AreEqual(MatcherType.type, matchingRules["$.anArray[*]"].Matchers.First().MatcherType);
            Assert.AreEqual(1, matchingRules["$.anArray"].Matchers.First().Min);
        }

        [TestMethod]
        public void ArrayWithStarAndMinTwoElements()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Array.Named("anArray").ContainingAtLeast(2).InWhichEveryElementIsLike(Some.Element.Like("Hello world")));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(2, matchingRules.Count);
            Assert.AreEqual(MatcherType.type, matchingRules["$.anArray[*]"].Matchers.First().MatcherType);
            Assert.AreEqual(2, matchingRules["$.anArray"].Matchers.First().Min);
        }

        [TestMethod]
        public void Integer()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Integer.Like(1));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(MatcherType.integer, matchingRules["$"].Matchers.First().MatcherType);
        }

        [TestMethod]
        public void Decimal()
        {
            var pactJsonBody = Pact.JsonContent.With(Some.Decimal.Like(1.1));

            var matchingRules = pactJsonBody.CreateV3MatchingRules();

            Assert.AreEqual(MatcherType.@decimal, matchingRules["$"].Matchers.First().MatcherType);
        }
    }
}
