using ComPact.Matchers.Regex;
using ComPact.Matchers.Type;

namespace ComPact.Matchers
{
    public static class Match
    {
        public static IMatcher Regex(string example, string regex)
        {
            return new RegexMatcher(example, regex);
        }

        public static IMatcher Type(dynamic example)
        {
            return new TypeMatcher(example);
        }

        public static IMatcher MinType(dynamic example, int min)
        {
            return new MinTypeMatcher(example, min);
        }
    }
}