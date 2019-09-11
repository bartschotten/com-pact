using ComPact.Models.V3;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComPact.Mock.Provider
{
    internal class RequestResponseMatchingErrorResponse
    {
        [JsonProperty("message")]
        internal string Message { get; set; }
        [JsonProperty("actualRequests")]
        internal Request ActualRequest { get; set; }
        [JsonProperty("expectedRequests")]
        internal List<Request> ExpectedRequests { get; set; }
    }
}
