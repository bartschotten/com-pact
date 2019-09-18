using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComPact.Verifier
{
    public class VerificationResults
    {
        [JsonProperty("providerName")]
        public string ProviderName { get; set; }
        [JsonProperty("providerApplicationVersion")]
        public string ProviderApplicationVersion { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("verificationDate")]
        public string VerificationDate { get; set; }
        [JsonProperty("testResults")]
        public TestResults TestResults { get; set; }
    }

    public class TestResults
    {
        [JsonProperty("summary")]
        public Summary Summary { get; set; }
        [JsonProperty("tests")]
        public List<Test> Tests { get; set; }
    }

    public class Summary
    {
        [JsonProperty("testCount")]
        public int TestCount { get; set; }
        [JsonProperty("failureCount")]
        public int FailureCount { get; set; }
    }

    public class Test
    {
        [JsonProperty("testDescription")]
        public string Description { get; set; }
        [JsonProperty("success")]
        public string Status => Issues.Any() ? "failed" : "passed";
        [JsonProperty("issues")]
        public List<string> Issues { get; set; } = new List<string>();

        public string ToTestMessageString()
        {
            var stringBuilder = new StringBuilder(Description);
            stringBuilder.Append($" ({Status})");
            if (Status == "failed")
            {
                stringBuilder.Append(":");
                foreach (var issues in Issues)
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("- ");
                    stringBuilder.Append(issues);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
