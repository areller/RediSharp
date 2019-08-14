using System;

namespace RediSharp.Lua
{
    public class LuaMismatchReturnTypeException : Exception
    {
        public LuaMismatchReturnTypeException(
            Type expectedType,
            Type returnedType)
            : base($"Expected return type '{expectedType}', but got '{returnedType}'")
        { }
    }
}