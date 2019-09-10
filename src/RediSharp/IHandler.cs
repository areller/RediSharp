using System.Threading.Tasks;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL;

namespace RediSharp
{
    interface IHandler<TArtifact>
    {
        IHandle<TRes> CreateHandle<TRes>(RootNode redIL);
    }
}