using System;

namespace RediSharp.CSharp
{
    public class DecompilationException : Exception
    {
        public DecompilationException(string message)
            : base(message)
        { }
    }
}