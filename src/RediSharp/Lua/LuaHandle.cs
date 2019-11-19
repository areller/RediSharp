using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;
using StackExchange.Redis;

namespace RediSharp.Lua
{
    class LuaHandle<TRes> : IHandle<TRes>, IDisposable
    {
        #region Static
        
        private static readonly RedisKey[] _EmptyKeys = new RedisKey[0];
        private static readonly RedisValue[] _EmptyArgs = new RedisValue[0];
        
        #endregion
        
        private IDatabase _db;

        private string _hash;

        private Func<RedisResult, object> _converter;

        public LuaHandle(
            IDatabase db,
            string script,
            Func<RedisResult, object> converter)
        {
            _db = db;
            _converter = converter;
            Artifact = script;
            IsInitialized = false;
        }

        public object Artifact { get; }

        public bool IsInitialized { get; private set; }

        public async Task Init()
        {
            var res = await _db.ExecuteAsync("SCRIPT", new
                List<object>() {"LOAD", Artifact}).ConfigureAwait(false);

            _hash = (string) res;
            IsInitialized = true;
        }

        public async Task<TRes> Execute(RedisValue[] args, RedisKey[] keys)
        {
            if (!IsInitialized)
            {
                throw new HandleException("Handle was not initialized");
            }
            
            args = args ?? _EmptyArgs;
            keys = keys ?? _EmptyKeys;
            
            var result = await _db.ExecuteAsync("EVALSHA",
                new object[] {_hash, keys.Length}.Concat(keys.Select(k => (object)k)).Concat(args.Select(a => (object)a)).ToArray());
            
            var parsedResult = ParseResult(result);
            return parsedResult;
        }

        private TRes ParseResult(RedisResult nativeRedisResult)
        {
            var res = _converter(nativeRedisResult);
            return (TRes) res;
        }

        public void Dispose()
        {
            ;
        }
    }
}