using ComPact.Models;

namespace ComPact.Builders.V3
{
    public class UnknownInteger
    {
        public SimpleValueName Named(string name) => new SimpleValueName(name);
        public SimpleValue Like(int example) => new SimpleValue(example, MatcherType.integer);
    }

    public class UnknownDecimal
    {
        public SimpleValueName Named(string name) => new SimpleValueName(name);
        public SimpleValue Like(double example) => new SimpleValue(example, MatcherType.@decimal);
    }
}
