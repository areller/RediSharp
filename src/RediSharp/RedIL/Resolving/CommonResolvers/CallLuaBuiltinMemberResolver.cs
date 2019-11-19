using System.Collections.Generic;
using RediSharp.Lua;
using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving.CommonResolvers
{
    class CallLuaBuiltinMemberResolver : RedILMemberResolver
    {
        private LuaBuiltinMethod _method;

        public CallLuaBuiltinMemberResolver(object arg)
        {
            _method = (LuaBuiltinMethod) arg;
        }
        
        public override ExpressionNode Resolve(Context context, ExpressionNode caller)
        {
            return new CallBuiltinLuaMethodNode(_method, new List<ExpressionNode>() {caller});
        }
    }
}