using StackExchange.Redis;

namespace RediSharp
{
    public interface IRedisProcedure<TRes>
    {
        TRes Define(IDatabase client, RedisValue[] args, RedisKey[] keys);
    }
}