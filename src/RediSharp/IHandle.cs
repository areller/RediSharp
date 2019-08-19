using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp
{
    public interface IHandle<TRes>
    {
        bool IsInitialized { get; }

        Task Init();

        Task<TRes> Execute(RedisValue[] args, RedisKey[] keys);
    }
}