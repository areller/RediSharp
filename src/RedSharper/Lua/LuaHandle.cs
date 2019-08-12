using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedSharper.Contracts;
using RedSharper.Contracts.Enums;
using StackExchange.Redis;

namespace RedSharper.Lua
{
    class LuaHandle : IHandle<string>, IDisposable
    {
        private IDatabase _db;

        private string _hash;

        public LuaHandle(
            IDatabase db,
            string script)
        {
            _db = db;
            Artifact = script;
            IsInitialized = false;
        }

        public string Artifact { get; }

        public bool IsInitialized { get; private set; }

        public async Task Init()
        {
            var res = await _db.ExecuteAsync("SCRIPT", new
                List<object>() {"LOAD", Artifact}).ConfigureAwait(false);

            _hash = (string) res;
            IsInitialized = true;
        }

        public async Task<TRes> Execute<TRes>(RedisValue[] args, RedisKey[] keys)
            where TRes : RedResult
        {
            var result = await _db.ExecuteAsync("EVALSHA",
                new object[] {_hash, keys.Length}.Concat(keys.Select(k => (object)k)).Concat(args.Select(a => (object)a)).ToArray());
            var parsedResult = ParseResult(result);

            if (!parsedResult.GetType().Equals(typeof(TRes)))
            {
                throw new LuaMismatchReturnTypeException(typeof(TRes), parsedResult.GetType());
            }

            return (TRes)parsedResult;
        }

        private RedResult ParseResult(RedisResult nativeRedisResult)
        {
            switch (nativeRedisResult.Type)
            {
                case ResultType.Error: 
                    return new RedErrorResult(nativeRedisResult.ToString());
                case ResultType.Integer:
                case ResultType.SimpleString:
                case ResultType.BulkString:
                    return new RedSingleResult((RedisValue)nativeRedisResult, ParseResultType(nativeRedisResult.Type));
                case ResultType.MultiBulk:
                    var nativeArray = (RedisResult[])nativeRedisResult;
                    return new RedArrayResult(nativeArray.Select(nativeResult => ParseResult(nativeResult)).ToArray());
                default: return null;
            }
        }

        private RedResultType ParseResultType(ResultType nativeResultType)
        {
            switch (nativeResultType)
            {
                case ResultType.BulkString: return RedResultType.BulkString;
                case ResultType.Error: return RedResultType.Error;
                case ResultType.Integer: return RedResultType.Integer;
                case ResultType.MultiBulk: return RedResultType.MultiBulk;
                case ResultType.SimpleString: return RedResultType.SimpleString;
                default: return RedResultType.None;
            }
        }

        public void Dispose()
        {
            ;
        }
    }
}