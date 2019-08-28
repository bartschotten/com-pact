using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.UnitTests.Matching
{
    [TestClass]
    public class TryGetApplicableMatcherListTests
    {
        private MatcherList _typeMatcherList = new MatcherList { Matchers = new List<Matcher> { new Matcher { MatcherType = MatcherType.type } } };
        private MatcherList _equalityMatcherList = new MatcherList { Matchers = new List<Matcher> { new Matcher { MatcherType = MatcherType.equality } } };
        private MatcherList _integerMatcherList = new MatcherList { Matchers = new List<Matcher> { new Matcher { MatcherType = MatcherType.integer } } };

        [TestMethod]
        public void ShouldOrderPathsByLengthThenByLeastNumberOfStars()
        {
            var paths = new List<string>
            {
                "$.object.a.name",
                "$.object.*.name",
                "$.object.*",
                "$.object.a"
            };

            var orderedPaths = paths.OrderBy(p => p, new MatchingRulePathComparer()).ToList();

            Assert.AreEqual("$.object.*", orderedPaths[0]);
            Assert.AreEqual("$.object.a", orderedPaths[1]);
            Assert.AreEqual("$.object.*.name", orderedPaths[2]);
            Assert.AreEqual("$.object.a.name", orderedPaths[3]);
        }

        [TestMethod]
        public void ShouldFindForRoot()
        {
            var token = JToken.FromObject("test");
            var collection = new MatchingRuleCollection { Body = new Dictionary<string, MatcherList> { { "$", _typeMatcherList } } };

            var isFound = collection.TryGetApplicableMatcherListForToken(token.Root, out var applicableMatcherList);

            Assert.IsTrue(isFound);
            Assert.AreEqual(_typeMatcherList, applicableMatcherList);
        }

        [TestMethod]
        public void ShouldFindForParentToken()
        {
            var token = JToken.FromObject(new { name = "test" });
            var collection = new MatchingRuleCollection { Body = new Dictionary<string, MatcherList> { { "$", _typeMatcherList } } };

            var isFound = collection.TryGetApplicableMatcherListForToken(token.Children().First(), out var applicableMatcherList);

            Assert.IsTrue(isFound);
            Assert.AreEqual(_typeMatcherList, applicableMatcherList);
        }

        [TestMethod]
        public void ShouldNotFindForChildToken()
        {
            var token = JToken.FromObject(new { name = "test" });
            var collection = new MatchingRuleCollection { Body = new Dictionary<string, MatcherList> { { "$.name", _typeMatcherList } } };

            var isFound = collection.TryGetApplicableMatcherListForToken(token.Root, out var applicableMatcherList);

            Assert.IsFalse(isFound);
        }

        [TestMethod]
        public void ShouldNotFindForSiblingToken()
        {
            var token = JToken.FromObject(new { name = "test", number = 1 });
            var collection = new MatchingRuleCollection { Body = new Dictionary<string, MatcherList> { { "$.name", _typeMatcherList } } };

            var isFound = collection.TryGetApplicableMatcherListForToken(token.SelectToken("number"), out var applicableMatcherList);

            Assert.IsFalse(isFound);
        }

        [TestMethod]
        public void ShouldFindMostSpecificMatcherList()
        {
            var token = JToken.FromObject(new { name = "test" });
            var collection = new MatchingRuleCollection { Body = new Dictionary<string, MatcherList> { { "$", _typeMatcherList }, { "$.name", _integerMatcherList } } };

            var isFound = collection.TryGetApplicableMatcherListForToken(token.Children().First(), out var applicableMatcherList);

            Assert.IsTrue(isFound);
            Assert.AreEqual(_integerMatcherList, applicableMatcherList);
        }

        [TestMethod]
        public void ShouldNotFindWhenEqualityMatcherListIsApplicable()
        {
            var token = JToken.FromObject(new { name = "test" });
            var collection = new MatchingRuleCollection { Body = new Dictionary<string, MatcherList> { { "$", _typeMatcherList }, { "$.name", _equalityMatcherList } } };

            var isFound = collection.TryGetApplicableMatcherListForToken(token.Children().First(), out var applicableMatcherList);

            Assert.IsFalse(isFound);
        }

        [TestMethod]
        public void ShouldFindWithPathEndingInStar()
        {
            var token = JToken.FromObject(new { name = "test" });
            var collection = new MatchingRuleCollection { Body = new Dictionary<string, MatcherList> { { "$.*", _typeMatcherList } } };

            var isFound = collection.TryGetApplicableMatcherListForToken(token.Children().First(), out var applicableMatcherList);

            Assert.IsTrue(isFound);
            Assert.AreEqual(_typeMatcherList, applicableMatcherList);
        }

        [TestMethod]
        public void ShouldFindWithStarAnywhereInPath()
        {
            var token = JToken.FromObject(new { thing = new { name = "test" } });
            var collection = new MatchingRuleCollection { Body = new Dictionary<string, MatcherList> { { "$.*.name", _typeMatcherList } } };

            var isFound = collection.TryGetApplicableMatcherListForToken(token.SelectToken("thing.name"), out var applicableMatcherList);

            Assert.IsTrue(isFound);
            Assert.AreEqual(_typeMatcherList, applicableMatcherList);
        }

        [TestMethod]
        public void ShouldIgnoreStarWhenMoreSpecificOptionExists()
        {
            var token = JToken.FromObject(new { name = "test" });
            var collection = new MatchingRuleCollection { Body = new Dictionary<string, MatcherList> { { "$.*", _typeMatcherList }, { "$.name", _integerMatcherList } } };

            var isFound = collection.TryGetApplicableMatcherListForToken(token.Children().First(), out var applicableMatcherList);

            Assert.IsTrue(isFound);
            Assert.AreEqual(_integerMatcherList, applicableMatcherList);
        }
    }
}
