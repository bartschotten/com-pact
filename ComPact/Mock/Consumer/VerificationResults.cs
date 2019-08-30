using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComPact.Mock.Consumer
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
        [JsonProperty("failedInteractions")]
        public List<FailedInteraction> FailedInteractions{ get; set; }
    }

    public class FailedInteraction
    {
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("differences")]
        public List<string> Differences { get; set; }

        public string ToTestMessageString()
        {
            var stringBuilder = new StringBuilder(Description);
            stringBuilder.Append(":");
            foreach (var difference in Differences)
            {
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("- ");
                stringBuilder.Append(difference);
            }
            return stringBuilder.ToString();
        }
    }
}
