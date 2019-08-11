using System;
using StackExchange.Redis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RedSharper.CSharp;
using RedSharper.Contracts;
using RedSharper.RedIL;
using System.Diagnostics;
using RedSharper.Lua;

namespace RedSharper
{
    public class Client
    {
        private ActionDecompiler _decompiler;

        private CSharpCompiler _csharpCompiler;

        public Client(IConnectionMultiplexer connection)
        {
            _decompiler = new ActionDecompiler();
            _csharpCompiler = new CSharpCompiler();
        }

        public async Task<TRes> Execute<TRes>(Func<Cursor, RedisValue[], RedisKey[], TRes> action, RedisValue[] arguments = null, RedisKey[] keys = null)
            where TRes : RedResult
        {
            var decompilation = _decompiler.Decompile(action);
            var redIL = _csharpCompiler.Compile(decompilation);

            var luaCompiler = new LuaCompiler();
            var lua = luaCompiler.Compile(redIL);

            return null;
        }
    }
}