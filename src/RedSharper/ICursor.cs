using System;
using RedSharper.Contracts;
using StackExchange.Redis;

namespace RedSharper
{
    public interface ICursor
    {
        RedSingleResult Get(RedisKey key);

        RedResult Set(RedisKey key, RedisValue value, TimeSpan? expiry = null);
    }
}