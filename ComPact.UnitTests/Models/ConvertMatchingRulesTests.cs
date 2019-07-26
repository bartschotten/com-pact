using ComPact.Matchers;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ComPact.UnitTests.Models
{
    [TestClass]
    public class ConvertMatchingRulesTests
    {
        [TestMethod]
        public void TypeMatcherOnSingleProperty()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers(),
                Body = new
                {
                    name = Match.Type("text")
                }
            };

            var convertedResponse = response.ConvertMatchingRules();

            Assert.AreEqual(1, convertedResponse.MatchingRules.Children().Count());
            Assert.AreEqual("$.body.name", ((JProperty)convertedResponse.MatchingRules.First).Name);
            Assert.AreEqual("text", ((JObject)convertedResponse.Body)["name"].Value<string>());
        }

        [TestMethod]
        public void TypeMatcherInArray()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers(),
                Body = new []
                {
                    Match.Type("text")
                }
            };

            var convertedResponse = response.ConvertMatchingRules();

            Assert.AreEqual(1, convertedResponse.MatchingRules.Children().Count());
            Assert.AreEqual("$.body[0]", ((JProperty)convertedResponse.MatchingRules.First).Name);
        }

        [TestMethod]
        public void TypeMatcherDirectlyOnBody()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers(),
                Body = Match.Type("text")
            };

            var convertedResponse = response.ConvertMatchingRules();

            Assert.AreEqual(1, convertedResponse.MatchingRules.Children().Count());
            Assert.AreEqual("$.body", ((JProperty)convertedResponse.MatchingRules.First).Name);
            Assert.AreEqual("text", convertedResponse.Body);
        }

        [TestMethod]
        public void MultipleMatchingRules()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers(),
                Body = new
                {
                    name = Match.Type("text"),
                    number = 5,
                    nestedObject = new
                    {
                        decimalNumber = Match.Type(1.5)
                    }
                }
            };

            var convertedResponse = response.ConvertMatchingRules();

            Assert.AreEqual(2, convertedResponse.MatchingRules.Children().Count());
            Assert.AreEqual("$.body.name", ((JProperty)convertedResponse.MatchingRules.First).Name);
            Assert.AreEqual("text", ((JObject)convertedResponse.Body)["name"].Value<string>());
            Assert.AreEqual("$.body.nestedObject.decimalNumber", ((JProperty)convertedResponse.MatchingRules.Last).Name);
            Assert.AreEqual(1.5, ((JObject)convertedResponse.Body)["nestedObject"]["decimalNumber"].Value<double>());
        }

        [TestMethod]
        public void MatchComplexObject()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers(),
                Body = new
                {
                    nestedObject = Match.Type(new
                    {
                        name = "text",
                        number = 5,
                    })
                }
            };

            var convertedResponse = response.ConvertMatchingRules();

            Assert.AreEqual(1, convertedResponse.MatchingRules.Children().Count());
            Assert.AreEqual("$.body.nestedObject", ((JProperty)convertedResponse.MatchingRules.First).Name);
        }

        [TestMethod]
        public void NestedMatchingRules()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers(),
                Body = new
                {
                    nestedObject = Match.Type(new
                    {
                        name = "text",
                        number = Match.Type(5),
                    })
                }
            };

            var convertedResponse = response.ConvertMatchingRules();

            Assert.AreEqual(2, convertedResponse.MatchingRules.Children().Count());
            Assert.AreEqual("$.body.nestedObject", ((JProperty)convertedResponse.MatchingRules.First).Name);
            Assert.AreEqual("$.body.nestedObject.number", ((JProperty)convertedResponse.MatchingRules.Last).Name);
        }

        [TestMethod]
        public void MatchingRuleOnArray()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers(),
                Body = new
                {
                    array = Match.MinType(new[]
                    {
                        5
                    }, 1)
                }
            };

            var convertedResponse = response.ConvertMatchingRules();

            Assert.AreEqual(1, convertedResponse.MatchingRules.Children().Count());
            Assert.AreEqual("$.body.array[*]", ((JProperty)convertedResponse.MatchingRules.First).Name);
        }

        [TestMethod]
        public void NestedMatchingRuleOnArray()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers(),
                Body = new
                {
                    array = Match.MinType(new []
                    {
                        Match.Type(5),
                    }, 2)
                }
            };

            var convertedResponse = response.ConvertMatchingRules();

            Assert.AreEqual(2, convertedResponse.MatchingRules.Children().Count());
            Assert.AreEqual("$.body.array[*]", ((JProperty)convertedResponse.MatchingRules.First).Name);
            Assert.AreEqual("$.body.array[0]", ((JProperty)convertedResponse.MatchingRules.Last).Name);
        }

        [TestMethod]
        public void RegexMatcherOnSingleProperty()
        {
            var response = new Response
            {
                Status = 200,
                Headers = new Headers(),
                Body = new
                {
                    name = Match.Regex("text", "^text$")
                }
            };

            var convertedResponse = response.ConvertMatchingRules();

            Assert.AreEqual(1, convertedResponse.MatchingRules.Children().Count());
            Assert.AreEqual("$.body.name", ((JProperty)convertedResponse.MatchingRules.First).Name);
            Assert.AreEqual("text", ((JObject)convertedResponse.Body)["name"].Value<string>());
        }
    }
}
