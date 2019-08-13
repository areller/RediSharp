using System.Threading.Tasks;
using RedSharper.Contracts;
using RedSharper.RedIL;

namespace RedSharper
{
    interface IHandler<TArtifact>
    {
        IHandle<TArtifact, TRes> CreateHandle<TRes>(RedILNode redIL)
            where TRes : RedResult;
    }
}