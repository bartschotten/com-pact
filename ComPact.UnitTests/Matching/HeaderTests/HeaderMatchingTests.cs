using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace ComPact.UnitTests.Matching.HeaderTests
{
    [TestClass]
    public class HeaderMatchingTests
    {
        [TestMethod]
        public void ShouldSuccessfullyExecuteAllTestcase()
        {
            var testcasesDir = Path.GetFullPath($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}.." +
                $"{Path.DirectorySeparatorChar}Matching{Path.DirectorySeparatorChar}HeaderTests{Path.DirectorySeparatorChar}Testcases{Path.DirectorySeparatorChar}");

            var testcaseFiles = Directory.GetFiles(testcasesDir);

            var failedCases = new List<string>();

            foreach(var file in testcaseFiles)
            {
                var testcase = JsonConvert.DeserializeObject<TestCase>(File.ReadAllText(file));
                var differences = testcase.Expected.Headers.Match(testcase.Actual.Headers, testcase.Expected.MatchingRules);
                if (differences.Any() == testcase.Match)
                {
                    failedCases.Add(testcase.Comment);
                }
            }

            Assert.AreEqual(0, failedCases.Count, "Failed cases: " + Environment.NewLine  + string.Join(Environment.NewLine, failedCases));
        }
    }
}
