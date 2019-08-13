using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using ComPact.Models;
using Newtonsoft.Json;
using System.Linq;

namespace ComPact.UnitTests.Matching.Header
{
    [TestClass]
    public class HeaderMatchingTests
    {
        [TestMethod]
        public void ShouldSuccessfullyExecuteAllTestcase()
        {
            var testcasesDir = Path.GetFullPath($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}.." +
                $"{Path.DirectorySeparatorChar}Matching{Path.DirectorySeparatorChar}Header{Path.DirectorySeparatorChar}Testcases{Path.DirectorySeparatorChar}");

            var testcasesFile = Directory.GetFiles(testcasesDir);

            var failedCases = new List<string>();

            foreach(var file in testcasesFile)
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

        private class TestCase
        {
            public bool Match { get; set; }
            public string Comment { get; set; }
            public ExpectedHeaders Expected { get; set; }
            public ActualHeaders Actual { get; set; }
        }

        private class ExpectedHeaders
        {
            public Headers Headers { get; set; }
            public MatchingRuleCollection MatchingRules { get; set; }
        }

        private class ActualHeaders
        {
            public Headers Headers { get; set; }
        }
    }
}
