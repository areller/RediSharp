using System;

namespace RedSharper.RedIL
{
    class RedILException : Exception
    {
        public RedILException(string message)
            : base(message)
        { }
    }
}