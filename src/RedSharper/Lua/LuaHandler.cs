using System.Threading.Tasks;
using RedSharper.RedIL;
using StackExchange.Redis;

namespace RedSharper.Lua
{
    class LuaHandler : IHandler
    {
        private IDatabase _db;

        private LuaCompiler _compiler;

        public LuaHandler(IDatabase db)
        {
            _db = db;
            _compiler = new LuaCompiler();
        }

        public async Task<IHandle> CreateHandle(RedILNode redIL)
        {
            var script = _compiler.Compile(redIL);
            var handle = new LuaHandle(_db, script);

            await handle.Init();

            return handle;
        }
    }
}