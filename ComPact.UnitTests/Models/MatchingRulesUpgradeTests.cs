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
                { "$.body.name", new Matcher { MatcherType = "regex", Regex = "\\w+" } },
                { "$.body.number", new Matcher { MatcherType = "type"} },
                { "$.headers.content-type", new Matcher { MatcherType = "type"} }
            };

            var newMatchingRules = new MatchingRuleCollection(oldMatchingRules);

            Assert.AreEqual(2, newMatchingRules.Body.Count);
            Assert.AreEqual(1, newMatchingRules.Header.Count);
            Assert.AreEqual("name", newMatchingRules.Body.First().Key);
            Assert.AreEqual("AND", newMatchingRules.Body.First().Value.Combine);
            Assert.AreEqual(1, newMatchingRules.Body.First().Value.Matchers.Count);
            Assert.AreEqual("regex", newMatchingRules.Body.First().Value.Matchers.First().MatcherType);
        }
    }
}
