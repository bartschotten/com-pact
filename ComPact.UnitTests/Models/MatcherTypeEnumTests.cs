using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class MatcherTypeEnumTests
    {
        [TestMethod]
        public void ShouldDeserializeTypeToType()
        {
            var jsonString = "{ \"match\":\"type\"}";

            var matcher = JsonConvert.DeserializeObject<Matcher>(jsonString);

            Assert.AreEqual(MatcherType.type, matcher.MatcherType);
        }

        [TestMethod]
        public void ShouldDeserializeRegexToRegex()
        {
            var jsonString = "{ \"match\":\"regex\"}";

            var matcher = JsonConvert.DeserializeObject<Matcher>(jsonString);

            Assert.AreEqual(MatcherType.regex, matcher.MatcherType);
        }

        [TestMethod]
        public void ShouldDeserializeUnknownValueToType()
        {
            var jsonString = "{ \"match\":\"number\"}";

            var matcher = JsonConvert.DeserializeObject<Matcher>(jsonString);

            Assert.AreEqual(MatcherType.type, matcher.MatcherType);
        }
    }
}
