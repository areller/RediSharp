using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RediSharp
{
    public interface IHandle<TArtifact, TRes> : IHandle<TRes>, IDisposable
    {
        TArtifact Artifact { get; }
    }
}