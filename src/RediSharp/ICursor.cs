using System;
using RediSharp.Contracts;
using RediSharp.Enums;
using RediSharp.RedIL.Attributes;
using StackExchange.Redis;

namespace RediSharp
{
    public interface ICursor
    {
        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.Get)]
        RedSingleResult Get(RedisKey key);

        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.HGet)]
        RedSingleResult HGet(RedisKey key, RedisValue field);

        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.HMGet)]
        RedArrayResult HMGet(RedisKey key, RedisValue[] fields);

        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.HGetAll)]
        RedArrayResult HGetAll(RedisKey key);
        
        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.Set)]
        RedStatusResult Set(RedisKey key, RedisValue value, TimeSpan? expiry = null);

        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.HSet)]
        RedStatusResult HSet(RedisKey key, RedisValue field, RedisValue value);
    }
}