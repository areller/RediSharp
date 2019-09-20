using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL;
using StackExchange.Redis;

namespace RediSharp.Lua
{
    class LuaHandler : IHandler<string>
    {
        #region Static

        private static readonly Dictionary<Type, Func<RedisResult, object>> _ResConverter =
            new Dictionary<Type, Func<RedisResult, object>>()
            {
                // int, int?, int[], IList<int>
                {typeof(int), r => (int) r},
                {typeof(int?), r => (int?) r},
                {typeof(int[]), r => (int[]) r},
                {typeof(IList<int>), r => (int[]) r},
                // long, long?, long[], IList<long>
                {typeof(long), r => (long) r},
                {typeof(long?), r => (long?) r},
                {typeof(long[]), r => (long[]) r},
                {typeof(IList<long>), r => (long[]) r},
                // ulong, ulong?, ulong[], IList<ulong>
                {typeof(ulong), r => (ulong) r},
                {typeof(ulong?), r => (ulong?) r},
                {typeof(ulong[]), r => (ulong[]) r},
                {typeof(IList<ulong>), r => (ulong[]) r},
                // double, double?, double[], IList<double>
                {typeof(double), r => (double) r},
                {typeof(double?), r => (double?) r},
                {typeof(double[]), r => (double[]) r},
                {typeof(IList<double>), r => (double[]) r},
                // decimal, decimal?, decimal[], IList<decimal>
                {typeof(decimal), r => (double) r},
                {typeof(decimal?), r => (double?) r},
                {typeof(decimal[]), r => (double[]) r},
                {typeof(IList<decimal>), r => (double[]) r},
                // float, float?, float[], IList<float>
                {typeof(float), r => (double) r},
                {typeof(float?), r => (double?) r},
                {typeof(float[]), r => (double[]) r},
                {typeof(IList<float>), r => (double[]) r},
                // bool, bool?, bool[], IList<bool>
                {
                    typeof(bool),
                    r => r.Type == ResultType.SimpleString || (r.Type == ResultType.Integer && r.ToString() == "1")
                },
                {typeof(bool?), r => !r?.IsNull},
                {typeof(bool[]), r => (bool[]) r},
                {typeof(IList<bool>), r => (bool[]) r},
                // string, string[], IList<string>
                {typeof(string), r => (string) r},
                {typeof(string[]), r => (string[]) r},
                {typeof(IList<string>), r => (string[]) r},
                // byte[], byte[][], IList<byte[]>
                {typeof(byte[]), r => (byte[]) r},
                {typeof(byte[][]), r => (byte[][]) r},
                {typeof(IList<byte[]>), r => (byte[][]) r},
                // RedisValue, RedisValue[], IList<RedisValue>
                {typeof(RedisValue), r => (RedisValue) r},
                {typeof(RedisValue[]), r => (RedisValue[]) r},
                {typeof(IList<RedisValue>), r => (RedisValue[]) r},
                // RedisKey, RedisKey[], IList<RedisKey>
                {typeof(RedisKey), r => (RedisKey) r},
                {typeof(RedisKey[]), r => (RedisKey[]) r},
                {typeof(IList<RedisKey>), r => (RedisKey[]) r}
            };
        
        #endregion
        
        private IDatabase _db;

        private LuaCompiler _compiler;

        public LuaHandler(IDatabase db)
        {
            _db = db;
            _compiler = new LuaCompiler();
        }
        
        public IHandle<TRes> CreateHandle<TRes>(RootNode redIL)
        {
            var resType = typeof(TRes);
            if (!_ResConverter.TryGetValue(resType, out var converter))
            {
                throw new NotSupportedException($"Type '{resType}' is not supported as a return type");
            }
            
            var script = _compiler.Compile(redIL);
            var handle = new LuaHandle<TRes>(_db, script, converter);

            return handle;
        }
    }
}