﻿using System;

namespace ComPact.Exceptions
{
    public class PactException: Exception
    {
        public PactException(string message): base(message)
        {
        }
        public PactException(string message, Exception exception) : base(message,exception)
        {
        }
    }
}
