using System;
using ICSharpCode.Decompiler.CSharp.Syntax;
using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL
{
    class RedILException : Exception
    {
        public RedILException(string message)
            : base(message)
        {
        }
    }
}