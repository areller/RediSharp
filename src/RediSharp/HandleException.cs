using System;

namespace RediSharp
{
    public class HandleException : Exception
    {
        public HandleException(string message)
            : base(message)
        {
        }
    }
}