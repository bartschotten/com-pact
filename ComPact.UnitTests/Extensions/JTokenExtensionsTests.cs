using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ComPact.Extensions;
using System.Linq;

namespace ComPact.UnitTests.Extensions
{
    [TestClass]
    public class JTokenExtensionsTests
    {
        [TestMethod]
        public void ShouldReturnFlatEnumerableBaseOnJTokenTree()
        {
            var someObject = new
            {
                name = "test",
                number = 1,
                someNestedObject = new
                {
                    decimalNumber = 1.1,
                    array = new[]
                    {
                        1,
                        2,
                        3
                    }
                }
            };

            var jToken = JToken.FromObject(someObject);
            var flatEnumerable = jToken.ThisTokenAndAllItsDescendants();

            Assert.AreEqual(14, flatEnumerable.Count());
        }
    }
}
