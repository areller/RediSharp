using System;
using System.Threading.Tasks;
using RedSharper.Contracts;
using StackExchange.Redis;

namespace RedSharper
{
    interface IHandle<TArtifact> : IDisposable
    {
        TArtifact Artifact { get; }

        bool IsInitialized { get; }

        Task Init();
        
        Task<TRes> Execute<TRes>(RedisValue[] args, RedisKey[] keys)
            where TRes : RedResult;
    }
}