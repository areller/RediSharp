using System;

namespace RediSharp.Generator.Resolving
{
    [ProxyType("RediSharp.Resolving", nameof(CallLuaFunctionAttribute))]
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