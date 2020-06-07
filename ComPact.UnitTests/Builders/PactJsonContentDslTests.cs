using ComPact.Builders;
using ComPact.Builders.V2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;

namespace ComPact.UnitTests.Builders
{
    [TestClass]
    public class PactJsonContentDslTests
    {
        [TestMethod]
        public void NestedObject()
        {
            var pactJsonBody = Pact.JsonContent.With
                (
                    Some.Object.Named("package").With
                    (
                        Some.Element.Like("Hello world").Named("greeting"), 
                        Some.Element.Like(1).Named("number")
                    )
                ).ToJToken();

            var expectedObject = new { package = new { greeting = "Hello world", number = 1 } };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void RootLevelArray()
        {
            var pactJsonBody = Pact.JsonContent.With
                (
                    Some.Array.Of
                    (
                        Some.Element.Like("a"),
                        Some.Element.Like("b")
                    ).ContainingAtLeast(2)
                ).ToJToken();

            var expectedObject = new[] { "a", "b" };

            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }

        [TestMethod]
        public void SimpleValueFromString()
        {
            AssertJson(Some.String.Named("Key").Like("Hello world"), new { Key = "Hello world" });
            AssertVariationsOf(Some.String.Like("Hello world"), "Hello world");
        }

        [TestMethod]
        public void StringWithExactValue()
        {
            AssertJson(Some.String.Named("Key").WithTheExactValue("Hello world"), new { Key = "Hello world" });
            AssertVariationsOf(Some.String.WithTheExactValue("Hello world"), "Hello world");
        }

        [TestMethod]
        public void GuidAsString()
        {
            AssertJson(Some.String.Named("Key").LikeGuid("e5dfa73c-4398-440a-8094-69e61326f7f9"), new { Key = "e5dfa73c-4398-440a-8094-69e61326f7f9" });
            AssertVariationsOf(Some.String.LikeGuid("e5dfa73c-4398-440a-8094-69e61326f7f9"), "e5dfa73c-4398-440a-8094-69e61326f7f9");
        }

        [TestMethod]
        public void GuidAsGuid()
        {
            AssertJson(Some.String.Named("Key").LikeGuid(Guid.Parse("e5dfa73c-4398-440a-8094-69e61326f7f9")), new { Key = "e5dfa73c-4398-440a-8094-69e61326f7f9" });
            AssertVariationsOf(Some.String.LikeGuid(Guid.Parse("e5dfa73c-4398-440a-8094-69e61326f7f9")), "e5dfa73c-4398-440a-8094-69e61326f7f9");
        }

        [TestMethod]
        public void DateTimeAsString()
        {
            AssertJson(Some.String.Named("Key").LikeDateTime("2020-06-01T13:05:30"), new { Key = "2020-06-01T13:05:30" });
            AssertVariationsOf(Some.String.LikeDateTime("2020-06-01T13:05:30"), "2020-06-01T13:05:30");
        }

        [TestMethod]
        public void ElementWithExactValueString()
        {
            AssertJson(Some.Element.Named("Key").WithTheExactValue("Hello world"), new { Key = "Hello world" });
            AssertVariationsOf(Some.Element.WithTheExactValue("Hello world"), "Hello world");
        }

        [TestMethod]
        public void ElementWithExactValueLong()
        {
            AssertJson(Some.Element.Named("Key").WithTheExactValue(1), new { Key = 1 });
            AssertVariationsOf(Some.Element.WithTheExactValue(1), 1);
        }

        [TestMethod]
        public void ElementWithExactValueBool()
        {
            AssertJson(Some.Element.Named("Key").WithTheExactValue(true), new { Key = true });
            AssertVariationsOf(Some.Element.WithTheExactValue(true), true);
        }

        [TestMethod]
        public void ElementWithExactValueDecimal()
        {
            AssertJson(Some.Element.Named("Key").WithTheExactValue(1.1m), new { Key = 1.1 });
            AssertVariationsOf(Some.Element.WithTheExactValue(1.1m), 1.1);
        }

        [TestMethod]
        public void ElementWithExactValueDouble()
        {
            AssertJson(Some.Element.Named("Key").WithTheExactValue(1.1), new { Key = 1.1 });
            AssertVariationsOf(Some.Element.WithTheExactValue(1.1), 1.1);
        }

        private void AssertVariationsOf(Element pactJsonBodyElement, object expectedObject)
        {
            var pactJsonBody = Pact.JsonContent.With(pactJsonBodyElement).ToJToken();
            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));

            pactJsonBody = Pact.JsonContent.With(pactJsonBodyElement.Named("Key")).ToJToken();
            Assert.AreEqual(JsonConvert.SerializeObject(new { Key = expectedObject }), JsonConvert.SerializeObject(pactJsonBody));
        }

        private void AssertJson(Member pactJsonBodyElement, object expectedObject)
        {
            var pactJsonBody = Pact.JsonContent.With(pactJsonBodyElement).ToJToken();
            Assert.AreEqual(JsonConvert.SerializeObject(expectedObject), JsonConvert.SerializeObject(pactJsonBody));
        }
    }
}
