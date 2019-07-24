namespace ComPact.Matchers.Regex
{
    public class RegexMatcher : IMatcher
    {
        public string Match { get; set; }
        public dynamic Example { get; set; }
        public string Regex { get; set; }

        internal RegexMatcher(string example, string regex)
        {
            Match = "Pact::Term";
            Example = example;
            Regex = regex;
        }
    }
}