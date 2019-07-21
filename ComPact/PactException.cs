using System;

namespace ComPact
{
    public class PactException: Exception
    {
        public PactException(string message): base(message)
        {
        }
    }
}
