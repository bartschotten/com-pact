using ComPact.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ComPact.Builders
{
    internal static class PactWriter
    {
        public static void Write(IContract pact)
        {
#if USE_NET4X
            var buildDirectory = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""))).LocalPath;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
#else
            var buildDirectory = AppContext.BaseDirectory;
            var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
#endif
            Write(pact, pactDir);
        }

        public static void Write(IContract pact, string pactDir)
        {
            pact.SetEmptyValuesToNull();

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            var serializedPact = JsonConvert.SerializeObject(pact, settings);

            Directory.CreateDirectory(pactDir);
            File.WriteAllText($"{pactDir}{pact.Consumer.Name}-{pact.Provider.Name}.json", serializedPact);
        }
    }
}
