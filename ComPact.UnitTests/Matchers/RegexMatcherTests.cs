using ComPact.Matchers.Regex;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComPact.UnitTests.Matchers
{
    [TestClass]
    public class RegexMatcherTests
    {
        [TestMethod]
        public void ShouldCreateRegexMatcherBasedOnValidInput()
        {
            var matcher = new RegexMatcher("test", "^test$");

            Assert.AreEqual("test", matcher.Example);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowWhenExampleDoesNotMatchRegex()
        {
            try
            {
                new RegexMatcher("test", "^wrong$");
            }
            catch (PactException e)
            {
                Assert.AreEqual("The provided example test does not match the regular expression ^wrong$.", e.Message);
                throw;
            }
        }
    }
}
