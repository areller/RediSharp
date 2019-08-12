using System.Threading.Tasks;
using RedSharper.Contracts;
using StackExchange.Redis;

namespace RedSharper
{
    public interface IHandle
    {
        bool IsInitialized { get; }

        Task Init();
        
        Task<TRes> Execute<TRes>(RedisValue[] args, RedisKey[] keys)
            where TRes : RedResult;
    }
}