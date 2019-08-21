using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ComPact.UnitTests.Matching
{
    [TestClass]
    public class MatcherTests
    {
        [TestMethod]
        public void TypeMatchForString()
        {
            var matcher = new Matcher { MatcherType = MatcherType.type };

            var differences = matcher.Match("expected", "actual");

            Assert.AreEqual(0, differences.Count);
        }

        [TestMethod]
        public void TypeMismatchForString()
        {
            var matcher = new Matcher { MatcherType = MatcherType.type };

            var differences = matcher.Match("expected", 1);

            Assert.AreEqual("Expected value of type String (like: 'expected') at , but was value of type Integer.", differences.First());
        }

        [TestMethod]
        public void RegexMatchForString()
        {
            var matcher = new Matcher { MatcherType = MatcherType.regex, Regex = "^ex.*$" };

            var differences = matcher.Match("expected", "example");

            Assert.AreEqual(0, differences.Count);
        }

        [TestMethod]
        public void TypeMismatchForRegex()
        {
            var matcher = new Matcher { MatcherType = MatcherType.regex, Regex = "^ex.*$" };

            var differences = matcher.Match("expected", "actual");

            Assert.AreEqual("Expected value matching '^ex.*$' (like: 'expected') at , but was 'actual'.", differences.First());
        }

        [TestMethod]
        public void ShouldReturnPathInMessage()
        {
            var matcher = new Matcher { MatcherType = MatcherType.type };

            var expectedObject = JToken.FromObject(new { body = new { name = "expected" } });
            var differences = matcher.Match(expectedObject.SelectToken("body.name"), 1);

            Assert.AreEqual("Expected value of type String (like: 'expected') at body.name, but was value of type Integer.", differences.First());
        }

        [TestMethod]
        public void ArrayWithTooFewItems()
        {
            var matcher = new Matcher { MatcherType = MatcherType.type, Min = 2 };

            var differences = matcher.Match(new [] { 1, 2 }, new[] { 1 } );

            Assert.AreEqual("Expected an array with at least 2 item(s) at , but was 1 items(s).", differences.First());
        }

        [TestMethod]
        public void ArrayWithTooManyItems()
        {
            var matcher = new Matcher { MatcherType = MatcherType.type, Min = 1, Max = 1 };

            var differences = matcher.Match(new[] { 1 }, new[] { 1, 2 });

            Assert.AreEqual("Expected an array with at most 1 item(s) at , but was 2 items(s).", differences.First());
        }
    }
}
