using ComPact.Matchers;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class ResponseMatchTests
    {
        [TestMethod]
        public void IdenticalResponses()
        {
            CompareExpectedAndActualBody(new { name = "Hello world!" }, new { name = "Hello world!" }, null);
        }

        [TestMethod]
        public void ValuesDoNotMatch()
        {
            CompareExpectedAndActualBody(new { name = "Hello world!" }, new { name = "Hello Pact!" }, "Expected Hello world! at body.name, but was Hello Pact!");
        }

        [TestMethod]
        public void PropertyCannotBeFound()
        {
            CompareExpectedAndActualBody(new { name = "Hello world!" }, new { text = "Hello world!" }, "Property body.name was not present in the actual response");
        }

        [TestMethod]
        public void TypeMatchForString()
        {
            CompareExpectedAndActualBody(new { name = Match.Type("Hello world!") }, new { name = "Hello Pact!" }, null);
        }

        [TestMethod]
        public void TypeMismatchForString()
        {
            CompareExpectedAndActualBody(new { name = Match.Type("Hello world!") }, new { name = 1 }, "Expected value of type String (like: Hello world!) at body.name, but was value of type Integer");
        }

        [TestMethod]
        public void TypeMatchForInteger()
        {
            CompareExpectedAndActualBody(new { number = Match.Type(1) }, new { number = 2 }, null);
        }

        [TestMethod]
        public void TypeMismatchForInteger()
        {
            CompareExpectedAndActualBody(new { number = Match.Type(1) }, new { number = 1.1 }, "Expected value of type Integer (like: 1) at body.number, but was value of type Float");
        }

        [TestMethod]
        public void TypeMismatchForFloat()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CompareExpectedAndActualBody(new { number = Match.Type(1.1) }, new { number = 1 }, "Expected value of type Float (like: 1.1) at body.number, but was value of type Integer");
        }

        [TestMethod]
        public void TypeMismatchForBoolean()
        {
            CompareExpectedAndActualBody(new { status = Match.Type(true) }, new { status = 1 }, "Expected value of type Boolean (like: True) at body.status, but was value of type Integer");
        }

        [TestMethod]
        public void TypeMismatchWithRuleOnParentObject()
        {
            CompareExpectedAndActualBody(Match.Type(new { name = "Hello world!" }), new { name = 1 }, "Expected value of type String (like: Hello world!) at body.name, but was value of type Integer");
        }

        [TestMethod]
        public void TypeMismatchWithRuleWithinArray()
        {
            CompareExpectedAndActualBody(new [] { Match.Type("Hello world!") }, new [] { 1 }, "Expected value of type String (like: Hello world!) at body[0], but was value of type Integer");
        }

        [TestMethod]
        public void TypeMismatchWithRuleOnParentArray()
        {
            CompareExpectedAndActualBody(Match.Type(new[] { "Hello world!" }), new[] { 1 }, "Expected value of type String (like: Hello world!) at body[0], but was value of type Integer");
        }

        private void CompareExpectedAndActualBody(dynamic expected, dynamic actual, string expectedDifference)
        {
            var expectedResponse = new Response
            {
                Status = 200,
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = expected
            };

            expectedResponse = expectedResponse.ConvertMatchingRules();

            var actualResponse = new Response
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
