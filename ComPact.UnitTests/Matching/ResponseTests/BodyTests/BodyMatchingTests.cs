using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ComPact.UnitTests.Matching.ResponseTests.BodyTests
{
    [TestClass]
    public class BodyMatchingTests
    {
        [TestMethod]
        public void ShouldSuccessfullyExecuteAllTestcases()
        {
            var testcasesDir = Path.GetFullPath($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}.." +
                $"{Path.DirectorySeparatorChar}Matching{Path.DirectorySeparatorChar}ResponseTests{Path.DirectorySeparatorChar}BodyTests{Path.DirectorySeparatorChar}Testcases{Path.DirectorySeparatorChar}");

            var testcaseFiles = Directory.GetFiles(testcasesDir);

            var failedCases = new List<string>();

            foreach (var file in testcaseFiles)
            {
                var testcase = JsonConvert.DeserializeObject<Testcase>(File.ReadAllText(file));
                if (file.Split(Path.DirectorySeparatorChar).Last().StartsWith(""))
                {
                    List<string> differences = Body.Match(testcase.Expected.Body, testcase.Actual.Body, testcase.Expected.MatchingRules);
                    if (differences.Any() == testcase.Match)
                    {
                        failedCases.Add(file.Split(Path.DirectorySeparatorChar).Last());
                        failedCases.AddRange(differences.Select(d => "- " + d));
                    }
                }
            }

            Assert.AreEqual(0, failedCases.Count, "Failed cases: " + Environment.NewLine + string.Join(Environment.NewLine, failedCases));
        }
    }
}
