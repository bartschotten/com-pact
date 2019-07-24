using Newtonsoft.Json;
using System;

namespace ComPact.Matchers.Type
{
    public class MinTypeMatcher : IMatcher
    {
        public string Match { get; set; }
        public dynamic Example { get; set; }
        public int Min { get; set; }

        public MinTypeMatcher(dynamic example, int min)
        {
            if (min < 1)
            {
                throw new ArgumentException("Min must be greater than 0");
            }

            Match = "Pact::ArrayLike";
            Example = example;
            Min = min;
        }
    }
}