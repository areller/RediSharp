using RedSharper.Contracts;
using RedSharper.Enums;

namespace RedSharper
{
    public class Cursor
    {
        internal TRes Call<TRes>(RedisCommand command, params object[] arguments)
            where TRes : RedResult
        {
            return null;
        }
    }
}