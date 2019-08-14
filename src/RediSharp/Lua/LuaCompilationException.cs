using System;

namespace RediSharp.Lua
{
    class LuaCompilationException : Exception
    {
        public LuaCompilationException(string message)
            : base(message)
        {
        }
    }
}