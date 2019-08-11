using System;

namespace RedSharper.Lua
{
    class LuaCompilationException : Exception
    {
        public LuaCompilationException(string message)
            : base(message)
        {
        }
    }
}