using Newtonsoft.Json;
using System;
using System.Collections;

namespace ComPact.Matchers.Type
{
    internal class ArrayMatcher : IMatcher
    {
        public string Match { get; set; }
        public dynamic Example { get; set; }
        public int Min { get; set; }

        public ArrayMatcher(IList example, int min)
        {
            if (min < 1)
            {
                throw new ArgumentException("Min must be greater than 0");
            }

            Match = "Pact.ArrayLike";
            Example = example;
            Min = min;
        }
    }
}