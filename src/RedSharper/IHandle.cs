using System.Threading.Tasks;
using RedSharper.Contracts;
using StackExchange.Redis;

namespace RedSharper
{
    public interface IHandle<TRes>
        where TRes : RedResult
    {
        bool IsInitialized { get; }

        Task Init();

        Task<TRes> Execute(RedisValue[] args, RedisKey[] keys);
    }
}