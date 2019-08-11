using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class ResponseMatchTests
    {
        [TestMethod]
        public void IdenticalResponses()
        {
            CompareExpectedAndActualBody(new { name = "Hello world!" }, null, new { name = "Hello world!" }, null);
        }

        [TestMethod]
        public void ValuesDoNotMatch()
        {
            CompareExpectedAndActualBody(new { name = "Hello world!" }, null, new { name = "Hello Pact!" }, "Expected Hello world! at body.name, but was Hello Pact!.");
        }

        [TestMethod]
        public void PropertyCannotBeFound()
        {
            CompareExpectedAndActualBody(new { name = "Hello world!" }, null, new { text = "Hello world!" }, "Property body.name was not present in the actual response.");
        }

        [TestMethod]
        public void TypeMatchForString()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body.name", new Matcher { Match = "type" } } };
            CompareExpectedAndActualBody(new { name = "Hello world!" }, matchingRules, new { name = "Hello Pact!" }, null);
        }

        [TestMethod]
        public void TypeMismatchForString()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body.name", new Matcher { Match = "type" } } };
            CompareExpectedAndActualBody(new { name = "Hello world!" }, matchingRules, new { name = 1 }, "Expected value of type String (like: Hello world!) at body.name, but was value of type Integer.");
        }

        [TestMethod]
        public void TypeMatchForInteger()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body.number", new Matcher { Match = "type" } } };
            CompareExpectedAndActualBody(new { number = 1 }, matchingRules, new { number = 2 }, null);
        }

        [TestMethod]
        public void TypeMismatchForInteger()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body.number", new Matcher { Match = "type" } } };
            CompareExpectedAndActualBody(new { number = 1 }, matchingRules, new { number = 1.1 }, "Expected value of type Integer (like: 1) at body.number, but was value of type Float.");
        }

        [TestMethod]
        public void TypeMismatchForFloat()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var matchingRules = new Dictionary<string, Matcher> { { "$.body.number", new Matcher { Match = "type" } } };
            CompareExpectedAndActualBody(new { number = 1.1 }, matchingRules, new { number = 1 }, "Expected value of type Float (like: 1.1) at body.number, but was value of type Integer.");
        }

        [TestMethod]
        public void TypeMismatchForBoolean()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body.status", new Matcher { Match = "type" } } };
            CompareExpectedAndActualBody(new { status = true }, matchingRules, new { status = 1 }, "Expected value of type Boolean (like: True) at body.status, but was value of type Integer.");
        }

        [TestMethod]
        public void TypeMismatchWithRuleOnParentObject()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body", new Matcher { Match = "type" } } };
            CompareExpectedAndActualBody(new { name = "Hello world!" }, matchingRules, new { name = 1 }, "Expected value of type String (like: Hello world!) at body.name, but was value of type Integer.");
        }

        [TestMethod]
        public void TypeMismatchWithRuleWithinArray()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body[0]", new Matcher { Match = "type" } } };
            CompareExpectedAndActualBody(new [] { "Hello world!" }, matchingRules, new [] { 1 }, "Expected value of type String (like: Hello world!) at body[0], but was value of type Integer.");
        }

        [TestMethod]
        public void TypeMismatchWithRuleOnParentArray()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body[0]", new Matcher { Match = "type" } } };
            CompareExpectedAndActualBody(new[] { "Hello world!" }, matchingRules, new[] { 1 }, "Expected value of type String (like: Hello world!) at body[0], but was value of type Integer.");
        }

        [TestMethod]
        public void NotEnoughItemsInArray()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body", new Matcher { Match = "type", Min = 2 } } };
            CompareExpectedAndActualBody(new[] { 1 }, matchingRules, new[] { 1 }, "Expected an array with 2 item(s) at body, but was 1 items(s).");
        }

        [TestMethod]
        public void RegexMatchForString()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body.name", new Matcher { Match = "regex", Regex = "^Hello.*$" } } };
            CompareExpectedAndActualBody(new { name = "Hello world!" }, matchingRules, new { name = "Hello Pact!" }, null);
        }

        [TestMethod]
        public void RegexMismatchForString()
        {
            var matchingRules = new Dictionary<string, Matcher> { { "$.body.name", new Matcher { Match = "regex", Regex = "^.*world!$" } } };
            CompareExpectedAndActualBody(new { name = "Hello world!" }, matchingRules, new { name = "Hello Pact!" }, "Expected value matching ^.*world!$ (like: Hello world!) at body.name, but was Hello Pact!.");
        }

        [TestMethod]
        public void MultipleMatchingRules()
        {
            var matchingRules = new Dictionary<string, Matcher>
            {
                { "$.body[0].name", new Matcher { Match = "type" } },
                { "$.body", new Matcher { Match = "type" } },
                { "$.body[*].name", new Matcher { Match = "type" } },
            };
            CompareExpectedAndActualBody(new[] { new { name = "Hello world!" } }, matchingRules, new[] { new { name = 1 } }, "Expected value of type String (like: Hello world!) at body[0].name, but was value of type Integer.");
        }

        [DataTestMethod]
        [DataRow("$.body.anObject.anArray[0].name")]
        [DataRow("$.body.anObject.anArray[*].name")]
        [DataRow("$.body.*.anArray[*].name")]
        public void ComplexStarNotation(string path)
        {
            var matchingRules = new Dictionary<string, Matcher> { { path, new Matcher { Match = "type" } } };
            var expectedBody = new
            {
                anObject = new
                {
                    anArray = new[] 
                    {
                        new
                        {
                            name = "Hello world!"
                        }
                    }
                }
            };

            var actualBody = new
            {
                anObject = new
                {
                    anArray = new[]
                    {
                        new
                        {
                            name = "Hello pact!"
                        }
                    }
                }
            };

            CompareExpectedAndActualBody(expectedBody, matchingRules, actualBody, null);
        }

        private void CompareExpectedAndActualBody(object expected, Dictionary<string, Matcher> matchingRules, object actual, string expectedDifference)
        {
            var expectedResponse = new ResponseV2
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = expected,
                MatchingRules = matchingRules ?? new Dictionary<string, Matcher>()
            };

            var actualResponse = new ResponseV2
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = actual
            };

            var differences = expectedResponse.Match(actualResponse);

            Assert.AreEqual(expectedDifference, differences.FirstOrDefault());
        }
    }
}
