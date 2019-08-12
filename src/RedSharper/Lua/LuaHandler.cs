using System.Threading.Tasks;
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
        
        public IHandle<string> CreateHandle(RedILNode redIL)
        {
            var script = _compiler.Compile(redIL);
            var handle = new LuaHandle(_db, script);

            return handle;
        }
    }
}