using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComPact.Models
{
    public class Response
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }
        [JsonProperty("body")]
        public dynamic Body { get; set; }
    }
}
