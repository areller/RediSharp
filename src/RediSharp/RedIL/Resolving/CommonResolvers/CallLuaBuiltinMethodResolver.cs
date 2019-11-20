using System.Collections.Generic;
using System.Linq;
using RediSharp.Lua;
using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving.CommonResolvers
{
    class CallLuaBuiltinMethodResolver : RedILMethodResolver
    {
        private LuaBuiltinMethod _method;

        public CallLuaBuiltinMethodResolver(object arg)
        {
            _method = (LuaBuiltinMethod) arg;
        }
        
        public override RedILNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments)
        {
            return new CallBuiltinLuaMethodNode(_method,
                new List<ExpressionNode>() {caller}.Concat(arguments).ToList());
        }
    }
}