using System;
using System.Collections.Generic;
using System.IO;

namespace ComPact
{
    internal class PactConfig
    {
        private string _pactDir;
        public string PactDir
        {
            get { return _pactDir; }
            set { _pactDir = ConvertToDirectory(value); }
        }

        private string _logDir;
        public string LogDir
        {
            get { return _logDir; }
            set { _logDir = ConvertToDirectory(value); }
        }

        public IEnumerable<IOutput> Outputters { get; set; }

        public PactConfig()
        {
#if USE_NET4X
            var buildDirectory = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", ""))).LocalPath;
            PactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            LogDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}logs{Path.DirectorySeparatorChar}");
#else
            var buildDirectory = AppContext.BaseDirectory;
            PactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            LogDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}logs{Path.DirectorySeparatorChar}");
#endif

            //The output can be directed elsewhere, however there isn't really anything interesting being written here.
            Outputters = new List<IOutput>();
        }

        private static string ConvertToDirectory(string path)
        {
            if (!path.EndsWith("/") && !path.EndsWith("\\"))
            {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}
