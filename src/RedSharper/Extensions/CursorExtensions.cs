using System;
using RedSharper.Contracts;
using RedSharper.Enums;
using StackExchange.Redis;

namespace RedSharper.Extensions
{
    public static class CursorExtensions
    {
        public static RedErrorResult HSet(this Cursor cursor, RedisKey key, RedisValue field, RedisValue value) => cursor.Call<RedErrorResult>(RedisCommand.HSet, key, field, value);

        public static RedArrayResult HGet(this Cursor cursor, RedisKey key, RedisValue[] fields) => cursor.Call<RedArrayResult>(RedisCommand.HMGet, key, fields);

        public static RedSingleResult HGet(this Cursor cursor, RedisKey key, RedisValue field) => cursor.Call<RedSingleResult>(RedisCommand.HGet, key, field);

        public static RedSingleResult Get(this Cursor cursor, RedisKey key) => cursor.Call<RedSingleResult>(RedisCommand.Get, key);

        public static RedErrorResult Set(this Cursor cursor, RedisKey key, RedisValue value, TimeSpan? expiry = null) => cursor.Call<RedErrorResult>(RedisCommand.Set, key, value, expiry);
    }
}