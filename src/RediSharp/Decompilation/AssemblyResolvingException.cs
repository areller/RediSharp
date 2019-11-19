using System;
using System.Collections;
using System.Collections.Generic;

namespace RediSharp.Decompilation
{
    public class AssemblyResolvingException : Exception
    {
        public override IDictionary Data { get; }

        public AssemblyResolvingException(string fullname)
            : base($"Assembly '{fullname}' could not be resolved")
        {
            Data = new Dictionary<string, object>()
            {
                {"fullname", fullname}
            };
        }
    }
}