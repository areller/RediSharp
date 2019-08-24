using System.Threading.Tasks;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL;
using StackExchange.Redis;

namespace RediSharp.Lua
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
        
        public IHandle<string, TRes> CreateHandle<TRes>(RootNode redIL)
        {
            var script = _compiler.Compile(redIL);
            var handle = new LuaHandle<TRes>(_db, script);

            return handle;
        }
    }
}