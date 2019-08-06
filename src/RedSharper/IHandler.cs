using System.Threading.Tasks;
using RedSharper.RedIL;

namespace RedSharper
{
    interface IHandler
    {
        Task<IHandle> CreateHandle(RedILNode redIL);
    }
}