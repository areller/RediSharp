using System;
using System.Threading.Tasks;
using RedSharper.Contracts;
using StackExchange.Redis;

namespace RedSharper
{
    public interface IHandle<TArtifact, TRes> : IHandle<TRes>, IDisposable
        where TRes : RedResult
    {
        TArtifact Artifact { get; }
    }
}