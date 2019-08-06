using System;
using RedSharper.Contracts;
using RedSharper.Enums;
using StackExchange.Redis;

namespace RedSharper.Extensions
{
    public static class CursorExtensions
    {
        public static RedSingleResult Get(this Cursor cursor, RedisKey key) => cursor.Call<RedSingleResult>(RedisCommand.Get, key);

        public static RedResult Set(this Cursor cursor, RedisKey key, RedisValue value, TimeSpan? expiry = null) => cursor.Call<RedResult>(RedisCommand.Set, key, value, expiry);
    }
}