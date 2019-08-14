using System.Threading.Tasks;
using RediSharp.Contracts;
using StackExchange.Redis;

namespace RediSharp
{
    public interface IHandle<TRes>
        where TRes : RedResult
    {
        bool IsInitialized { get; }

        Task Init();

        Task<TRes> Execute(RedisValue[] args, RedisKey[] keys);
    }
}