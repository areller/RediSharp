using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp.Lua
{
    class LuaHandle<TRes> : IHandle<TRes>, IDisposable
    {
        #region Static
        
        private static readonly RedisKey[] EmptyKeys = new RedisKey[0];

        private static readonly RedisValue[] EmptyArgs = new RedisValue[0];
        
        #endregion
        
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
            
            args = args ?? EmptyArgs;
            keys = keys ?? EmptyKeys;
            
            var result = await _db.ExecuteAsync("EVALSHA",
                new object[] {_hash, keys.Length}.Concat(keys.Select(k => (object)k)).Concat(args.Select(a => (object)a)).ToArray());
            
            var parsedResult = ParseResult<TRes>(result);
            return parsedResult;
        }

        private TRes ParseResult<TRes>(RedisResult nativeRedisResult)
        {
            object res = null;
            if (typeof(TRes) == typeof(string))
            {
                res = (string) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(byte[]))
            {
                res = (byte[]) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(double))
            {
                res = (double) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(long))
            {
                res = (long) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(ulong))
            {
                res = (ulong) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(int))
            {
                res = (int) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(bool))
            {
                res = nativeRedisResult.Type == ResultType.SimpleString;
                //res = (bool) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(RedisValue))
            {
                res = (RedisValue) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(RedisKey))
            {
                res = (RedisKey) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(double?))
            {
                res = (double?) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(long?))
            {
                res = (long?) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(ulong?))
            {
                res = (ulong?) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(int?))
            {
                res = (int?) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(bool?))
            {
                res = (bool?) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(string[]))
            {
                res = (string[]) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(byte[][]))
            {
                res = (byte[][]) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(double[]))
            {
                res = (double[]) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(long[]))
            {
                res = (long[]) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(ulong[]))
            {
                res = (ulong[]) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(int[]))
            {
                res = (int[]) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(bool[]))
            {
                res = (bool[]) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(RedisValue[]))
            {
                res = (RedisValue[]) nativeRedisResult;
            }
            //TODO: Handle rest of List<T>, IList<T>, Dictionary<K,V>, IDictionary<K,V> conversions
            else if (typeof(TRes) == typeof(List<RedisValue>))
            {
                res = ((RedisValue[]) nativeRedisResult).ToList();
            }
            else if (typeof(TRes) == typeof(RedisKey[]))
            {
                res = (RedisKey[]) nativeRedisResult;
            }
            else if (typeof(TRes) == typeof(HashEntry[]))
            {
                //TODO: Convert to HashEntry[]
            }
            else
            {
                throw new Exception("Mismatch of type");
            }

            return (TRes) res;
        }

        public void Dispose()
        {
            ;
        }
    }
}