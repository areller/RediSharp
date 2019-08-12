using System;
using RedSharper.Contracts;
using RedSharper.Enums;
using RedSharper.RedIL.Attributes;
using StackExchange.Redis;

namespace RedSharper.Extensions
{
    public static class CursorExtensions
    {
        #region Methods
        
        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.HSet)]
        public static RedStatusResult HSet(this Cursor cursor, RedisKey key, RedisValue field, RedisValue value) => cursor.Call<RedStatusResult>(RedisCommand.HSet, key, field, value);

        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.HMGet)]
        public static RedArrayResult HMGet(this Cursor cursor, RedisKey key, RedisValue[] fields) => cursor.Call<RedArrayResult>(RedisCommand.HMGet, key, fields);

        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.HGet)]
        public static RedSingleResult HGet(this Cursor cursor, RedisKey key, RedisValue field) => cursor.Call<RedSingleResult>(RedisCommand.HGet, key, field);

        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.Get)]
        public static RedSingleResult Get(this Cursor cursor, RedisKey key) => cursor.Call<RedSingleResult>(RedisCommand.Get, key);

        [RedILResolve(typeof(CursorRedisMethodResolver), RedisCommand.Set)]
        public static RedStatusResult Set(this Cursor cursor, RedisKey key, RedisValue value, TimeSpan? expiry = null) => cursor.Call<RedStatusResult>(RedisCommand.Set, key, value, expiry);
        
        #endregion
    }
}