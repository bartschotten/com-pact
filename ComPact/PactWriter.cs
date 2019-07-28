using ComPact.Models;
using Newtonsoft.Json;
using System.IO;

namespace ComPact
{
    internal static class PactWriter
    {
        public static void Write(PactV2 pact, PactConfig config)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            var serializedPact = JsonConvert.SerializeObject(pact, settings);

            Directory.CreateDirectory(config.PactDir);
            File.WriteAllText($"{config.PactDir}{pact.Consumer.Name}-{pact.Provider.Name}.json", serializedPact);
        }
    }
}
