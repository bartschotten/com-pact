using ComPact.Verifier;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ComPact.UnitTests.Verifier
{
    [TestClass]
    public class TestToTestMessageStringTests
    {
        [TestMethod]
        public void ShouldReturnDescriptionAndStatusForPassedTest()
        {
            var test = new Test
            {
                Description = "An interaction"
            };

            Assert.AreEqual("An interaction (passed)", test.ToTestMessageString());
        }

        [TestMethod]
        public void ShouldReturnDescriptionStatusAndListOfIssuesForFailedTest()
        {
            var test = new Test
            {
                Description = "A failed interaction",
                Issues = new List<string> { "issue 1", "issue 2" }
            };

            Assert.AreEqual("A failed interaction (failed):" + Environment.NewLine + "- issue 1" + Environment.NewLine + "- issue 2", test.ToTestMessageString());
        }
    }
}
