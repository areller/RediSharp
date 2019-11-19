using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace RediSharp.Decompilation
{
    public class AssemblyNotFoundException : Exception
    {
        public override IDictionary Data { get; }

        public AssemblyNotFoundException(Assembly assembly)
            : base($"Assembly '{assembly.FullName}' was not found")
        {
            Data = new Dictionary<string, object>()
            {
                {"assembly", assembly}
            };
        }
    }
}