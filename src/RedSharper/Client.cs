using System;
using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RedSharper.CSharp;
using RedSharper.Contracts;
using RedSharper.RedIL;
using System.Diagnostics;
using System.Reflection.Emit;
using RedSharper.Lua;
using RedSharper.RedIL.Nodes;

namespace RedSharper
{
    public class Client
    {
        private ActionDecompiler _decompiler;

        private CSharpCompiler _csharpCompiler;

        private LuaHandler _luaHandler;

        private ConcurrentDictionary<object, Lazy<RedILNode>> _redILCache;

        public Client(IDatabase db)
        {
            _decompiler = new ActionDecompiler();
            _csharpCompiler = new CSharpCompiler();
            _luaHandler = new LuaHandler(db);
        }

        public async Task<TRes> Execute<TRes>(Func<ICursor, RedisValue[], RedisKey[], TRes> action, RedisValue[] arguments = null, RedisKey[] keys = null)
            where TRes : RedResult
        {
            var handle = await GetInitializedHandle(action);
            var res = await handle.Execute(arguments, keys);

            return res;
        }

        public IHandle<TRes> GetHandle<TRes>(Func<ICursor, RedisValue[], RedisKey[], TRes> action)
            where TRes : RedResult
        {
            var decompilation = _decompiler.Decompile(action);
            var redIL = _csharpCompiler.Compile(decompilation);

            var handle = _luaHandler.CreateHandle<TRes>(redIL);

            return handle;
        }

        public IHandle<TArtifact, TRes> GetHandleWithArtifact<TRes, TArtifact>(Func<ICursor, RedisValue[], RedisKey[], TRes> action)
            where TRes : RedResult
        {
            var decompilation = _decompiler.Decompile(action);
            var redIL = _csharpCompiler.Compile(decompilation);

            var handler = SelectHandler<TArtifact>();

            return handler.CreateHandle<TRes>(redIL);
        }

        public IHandle<string, TRes> GetLuaHandle<TRes>(Func<ICursor, RedisValue[], RedisKey[], TRes> action)
            where TRes : RedResult
        {
            var decompilation = _decompiler.Decompile(action);
            var redIL = _csharpCompiler.Compile(decompilation);

            return _luaHandler.CreateHandle<TRes>(redIL);
        }

        public async Task<IHandle<TRes>> GetInitializedHandle<TRes>(Func<ICursor, RedisValue[], RedisKey[], TRes> action)
            where TRes : RedResult
        {
            var handle = GetHandle(action);
            await handle.Init();

            return handle;
        }

        private IHandler<TArtifact> SelectHandler<TArtifact>()
        {
            //For now only Lua
            return _luaHandler as IHandler<TArtifact>;
        }
    }
}