using System.Threading.Tasks;
using RedSharper.Contracts;
using StackExchange.Redis;

namespace RedSharper
{
    interface IHandle
    {
        Task<TRes> Execute<TRes>(RedisKey[] keys)
            where TRes : RedResult;
    }
}