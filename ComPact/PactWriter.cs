using ComPact.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComPact
{
    public static class PactWriter
    {
        public static void Write(PactV2 pact, PactConfig config)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            var serializedPact = JsonConvert.SerializeObject(pact, settings);

            try
            {
                Directory.CreateDirectory(config.PactDir);
                File.WriteAllText($"{config.PactDir}{pact.Consumer.Name}-{pact.Provider.Name}.json", serializedPact);
            }
            catch (Exception e)
            {
                var x = e;
            }
        }
    }
}
