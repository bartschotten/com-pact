using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class MatchingRulesUpgradeTests
    {
        [TestMethod]
        public void ShouldUpgradeOldMatchingRulesToNew()
        {
            var oldMatchingRules = new Dictionary<string, Matcher>()
            {
                { "$.body", new Matcher { MatcherType = MatcherType.type} },
                { "$.body.name", new Matcher { MatcherType = MatcherType.regex, Regex = "\\w+" } },
                { "$.headers.content-type", new Matcher { MatcherType = MatcherType.type} }
            };

            var newMatchingRules = new MatchingRuleCollection(oldMatchingRules);

            Assert.AreEqual(2, newMatchingRules.Body.Count);
            Assert.AreEqual(1, newMatchingRules.Body.First().Value.Matchers.Count);
            Assert.AreEqual(MatcherType.type, newMatchingRules.Body["$"].Matchers.First().MatcherType);
            Assert.AreEqual("\\w+", newMatchingRules.Body["$.name"].Matchers.First().Regex);
            Assert.AreEqual(MatcherType.regex, newMatchingRules.Body["$.name"].Matchers.First().MatcherType);

            Assert.AreEqual(1, newMatchingRules.Header.Count);
            Assert.AreEqual(MatcherType.type, newMatchingRules.Header["content-type"].Matchers.First().MatcherType);
        }
    }
}
