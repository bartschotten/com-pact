using System;

namespace ComPact.Exceptions
{
    public class PactVerificationException: Exception
    {
        public PactVerificationException(string message): base(message)
        {
        }
    }
}
