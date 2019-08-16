using ComPact.Models;

namespace ComPact.UnitTests.Matching
{
    internal class TestCase
    {
        public bool Match { get; set; }
        public string Comment { get; set; }
        public Expected Expected { get; set; }
        public Actual Actual { get; set; }
    }

    internal class Expected
    {
        public Headers Headers { get; set; }
        public dynamic Body { get; set; }
        public MatchingRuleCollection MatchingRules { get; set; }
    }

    internal class Actual
    {
        public Headers Headers { get; set; }
        public dynamic Body { get; set; }
    }
}
