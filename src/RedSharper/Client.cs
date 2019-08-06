using System;
using StackExchange.Redis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RedSharper.CSharp;
using RedSharper.Contracts;
using RedSharper.RedIL;
using System.Diagnostics;

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

        public Task Execute(Func<ICursor, RedisKey[], RedResult> action, RedisKey[] keys = null)
            => Execute<RedResult>(action, keys);

        public Task Execute<TArgs>(Func<ICursor, RedisKey[], TArgs, RedResult> action, TArgs args, RedisKey[] keys = null)
            where TArgs : struct
            => Execute<TArgs, RedResult>(action, args, keys);

        public async Task<TRes> Execute<TRes>(Func<ICursor, RedisKey[], TRes> action, RedisKey[] keys = null)
            where TRes : RedResult
        {
            _decompiler.Decompile(action);
            return null;
        }

        public async Task<TRes> Execute<TArgs, TRes>(Func<ICursor, RedisKey[], TArgs, TRes> action, TArgs args, RedisKey[] keys = null)
            where TArgs : struct
            where TRes : RedResult
        {
            var decompilation = _decompiler.Decompile(action);
            var redIL = _csharpCompiler.Compile(decompilation);

            return null;
        }
    }
}