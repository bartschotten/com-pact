using ComPact.Models.V3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ComPact.UnitTests.Models.V3
{
    [TestClass]
    public class QueryToAndFromQueryStringTests
    {
        [TestMethod]
        public void TwoSimpleParameters()
        {
            var inputQueryString ="skip=100&take=10";
            var query = new Query(inputQueryString);
            var outputQueryString = query.ToQueryString();

            Assert.AreEqual(inputQueryString, outputQueryString);
            Assert.AreEqual("100", query["skip"].First());
            Assert.AreEqual(1, query["skip"].Count());
            Assert.AreEqual("10", query["take"].First());
            Assert.AreEqual(1, query["skip"].Count());
        }

        [TestMethod]
        public void EmptyQueryString()
        {
            var inputQueryString = string.Empty;
            var query = new Query(inputQueryString);
            var outputQueryString = query.ToQueryString();

            Assert.AreEqual(inputQueryString, outputQueryString);
            Assert.AreEqual(0, query.Count());
        }

        [TestMethod]
        public void NullQueryString()
        {
            string inputQueryString = null;
            var query = new Query(inputQueryString);
            var outputQueryString = query.ToQueryString();

            Assert.AreEqual(string.Empty, outputQueryString);
            Assert.AreEqual(0, query.Count());
        }

        [TestMethod]
        public void OneSimpleParameter()
        {
            var inputQueryString = "skip=100";
            var query = new Query(inputQueryString);
            var outputQueryString = query.ToQueryString();

            Assert.AreEqual(inputQueryString, outputQueryString);
            Assert.AreEqual("100", query["skip"].First());
            Assert.AreEqual(1, query["skip"].Count());
        }

        [TestMethod]
        public void OneParameterWithMultipleValues()
        {
            var inputQueryString = "colors=red,blue";
            var query = new Query(inputQueryString);
            var outputQueryString = query.ToQueryString();

            Assert.AreEqual(inputQueryString, outputQueryString);
            Assert.AreEqual(2, query["colors"].Count());
            Assert.AreEqual("red", query["colors"][0]);
            Assert.AreEqual("blue", query["colors"][1]);
        }

        [TestMethod]
        public void MultipleParametersWithSameKeyAreJoined()
        {
            var inputQueryString = "color=red&color=blue";
            var query = new Query(inputQueryString);
            var outputQueryString = query.ToQueryString();

            Assert.AreEqual("color=blue,red", outputQueryString);
            Assert.AreEqual(2, query["color"].Count());
            Assert.AreEqual("blue", query["color"][0]);
            Assert.AreEqual("red", query["color"][1]);
        }
    }
}
