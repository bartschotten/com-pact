namespace ComPact.Matchers.Type
{
    public class TypeMatcher : IMatcher
    {
        public string Match { get; set; }
        public dynamic Example { get; set; }

        public TypeMatcher(dynamic example)
        {
            Match = "Pact::SomethingLike";
            Example = example;
        }
    }
}