using System;
using RediSharp.Enums;
using RediSharp.RedIL.Resolving.Attributes;
using StackExchange.Redis;

namespace RediSharp
{
    public interface ICursor
    {
        RedisValue Get(RedisKey key);

        bool Set(RedisKey key, RedisValue value, TimeSpan? expiry = null);
    }
}