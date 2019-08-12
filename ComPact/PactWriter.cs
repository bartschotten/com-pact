using ComPact.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ComPact
{
    internal static class PactWriter
    {
        public static void Write(PactV2 pact)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            var serializedPact = JsonConvert.SerializeObject(pact, settings);

#if USE_NET4X
            var buildDirectory = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""))).LocalPath;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
#else
            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
#endif

            Directory.CreateDirectory(pactDir);
            File.WriteAllText($"{pactDir}{pact.Consumer.Name}-{pact.Provider.Name}.json", serializedPact);
        }
    }
}
