using System;

namespace RedSharper.CSharp
{
    public class DecompilationException : Exception
    {
        public DecompilationException(string message)
            : base(message)
        { }
    }
}