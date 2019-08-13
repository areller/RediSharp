using System.Threading.Tasks;
using RedSharper.Contracts;
using RedSharper.RedIL;
using StackExchange.Redis;

namespace RedSharper.Lua
{
    class LuaHandler : IHandler<string>
    {
        private IDatabase _db;

        private LuaCompiler _compiler;

        public LuaHandler(IDatabase db)
        {
            _db = db;
            _compiler = new LuaCompiler();
        }
        
        public IHandle<string, TRes> CreateHandle<TRes>(RedILNode redIL)
            where TRes : RedResult
        {
            var script = _compiler.Compile(redIL);
            var handle = new LuaHandle<TRes>(_db, script);

            return handle;
        }
    }
}