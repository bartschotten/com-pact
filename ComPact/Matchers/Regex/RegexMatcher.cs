namespace ComPact.Matchers.Regex
{
    internal class RegexMatcher : IMatcher
    {
        public string Match { get; set; }
        public dynamic Example { get; set; }
        public string Regex { get; set; }

        public RegexMatcher(string example, string regex)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(example, regex))
            {
                throw new PactException($"The provided example {example} does not match the regular expression {regex}.");
            }

            Match = "Pact.Term";
            Example = example;
            Regex = regex;
        }
    }
}