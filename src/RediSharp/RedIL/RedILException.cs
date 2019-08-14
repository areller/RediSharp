using System;

namespace RediSharp.RedIL
{
    class RedILException : Exception
    {
        public RedILException(string message)
            : base(message)
        { }
    }
}