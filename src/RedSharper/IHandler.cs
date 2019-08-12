using System.Threading.Tasks;
using RedSharper.RedIL;

namespace RedSharper
{
    interface IHandler<TArtifact>
    {
        IHandle<TArtifact> CreateHandle(RedILNode redIL);
    }
}