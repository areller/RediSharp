using System;

namespace RediSharp.Resolving
{
    [AttributeUsage(AttributeTargets.Method)]
    class CallLuaFunctionAttribute : Attribute
    {
        public string Function { get; }
        public string[] Arguments { get; }

        public CallLuaFunctionAttribute(string function, string[] arguments)
        {
            Function = function;
            Arguments = arguments;
        }
    }
}