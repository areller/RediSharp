using System;
using System.Threading.Tasks;
using RediSharp.Contracts;
using StackExchange.Redis;

namespace RediSharp
{
    public interface IHandle<TArtifact, TRes> : IHandle<TRes>, IDisposable
        where TRes : RedResult
    {
        TArtifact Artifact { get; }
    }
}