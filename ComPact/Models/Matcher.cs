namespace ComPact.Models
{
    internal class Matcher
    {
        public string Match { get; set; }
        public uint? Min { get; set; }
        public uint? Max { get; set; }
        public string Regex { get; set; }
    }
}
