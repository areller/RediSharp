using System;
using System.Threading.Tasks;
using RedSharper.Contracts;
using StackExchange.Redis;

namespace RedSharper
{
    public interface IHandle<TArtifact> : IHandle, IDisposable
    {
        TArtifact Artifact { get; }
    }
}