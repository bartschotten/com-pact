using ComPact.Models.V3;

namespace ComPact.UnitTests.Matching.RequestTests
{
    internal class Testcase
    {
        public bool Match { get; set; }
        public string Comment { get; set; }
        public Request Expected { get; set; }
        public Request Actual { get; set; }
    }
}
