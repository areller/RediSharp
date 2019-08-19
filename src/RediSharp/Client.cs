using System;
using System.Reflection;
using RediSharp.CSharp;
using RediSharp.Lua;
using RediSharp.RedIL;
using StackExchange.Redis;

namespace RediSharp
{
    public class Client<TCursor>
    {
        private CSharpCompiler _csharpCompiler;

        private LuaHandler _luaHandler;

        private ActionDecompiler _decompiler;

        public Client()
            : this(null, Assembly.GetCallingAssembly())
        {
        }

        public Client(IDatabase db)
            : this(db, Assembly.GetCallingAssembly())
        {
        }

        internal Client(IDatabase db, Assembly assembly)
        {
            _csharpCompiler = new CSharpCompiler();
            _luaHandler = new LuaHandler(db);
            _decompiler = new ActionDecompiler(assembly);
        }

        public IHandle<string, TRes> GetLuaHandle<TRes>(Func<TCursor, RedisValue[], RedisKey[], TRes> action)
        {
            var decompilation = _decompiler.Decompile(action);
            var redIL = _csharpCompiler.Compile(decompilation);

            return _luaHandler.CreateHandle<TRes>(redIL);
        }
    }
}