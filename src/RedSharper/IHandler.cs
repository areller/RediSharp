using System.Threading.Tasks;
using RedSharper.Contracts;
using RedSharper.RedIL;
using RedSharper.RedIL.Nodes;

namespace RedSharper
{
    interface IHandler<TArtifact>
    {
        IHandle<TArtifact, TRes> CreateHandle<TRes>(RedILNode redIL)
            where TRes : RedResult;
    }
}