using StackExchange.Redis;

namespace RediSharp
{
    public delegate TRes Function<TCursor, TRes>(TCursor cursor, RedisValue[] args, RedisKey[] keys);
}