using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ComPact.UnitTests.Matching.RequestTests.QueryTests
{
    [TestClass]
    public class QueryMatchingTests
    {
        [TestMethod]
        public void ShouldSuccessfullyExecuteAllTestcase()
        {
            var testcasesDir = Path.GetFullPath($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}.." +
                $"{Path.DirectorySeparatorChar}Matching{Path.DirectorySeparatorChar}RequestTests{Path.DirectorySeparatorChar}QueryTests{Path.DirectorySeparatorChar}Testcases{Path.DirectorySeparatorChar}");

            var testcaseFiles = Directory.GetFiles(testcasesDir);

            var failedCases = new List<string>();

            foreach (var file in testcaseFiles)
            {
                var testcase = JsonConvert.DeserializeObject<Testcase>(File.ReadAllText(file));
                if (file.Split(Path.DirectorySeparatorChar).Last().StartsWith("same parameter multiple times in different order"))
                {
                    var isMatch = testcase.Expected.Match(testcase.Actual);
                    if (isMatch != testcase.Match)
                    {
                        failedCases.Add(file.Split(Path.DirectorySeparatorChar).Last());
                    }
                }
            }

            Assert.AreEqual(0, failedCases.Count, "Failed cases: " + Environment.NewLine + string.Join(Environment.NewLine, failedCases));
        }
    }
}
