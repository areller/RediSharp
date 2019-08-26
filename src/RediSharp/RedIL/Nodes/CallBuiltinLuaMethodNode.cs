using System.Collections.Generic;
using System.Linq;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
{
    class CallBuiltinLuaMethodNode : ExpressionNode
    {
        private static readonly Dictionary<LuaBuiltinMethod, DataValueType> MethodTypeTable
            = new Dictionary<LuaBuiltinMethod, DataValueType>()
            {
                {LuaBuiltinMethod.StringSub, DataValueType.String},
                {LuaBuiltinMethod.StringToLower, DataValueType.String},
                {LuaBuiltinMethod.StringToUpper, DataValueType.String},
                {LuaBuiltinMethod.StringLength, DataValueType.Integer},
                {LuaBuiltinMethod.StringFind, DataValueType.Integer},
                {LuaBuiltinMethod.StringGSub, DataValueType.String},
                {LuaBuiltinMethod.TableUnpack, DataValueType.Array},
                {LuaBuiltinMethod.TableInsert, DataValueType.Unknown},
                {LuaBuiltinMethod.TableRemove, DataValueType.Integer},
                {LuaBuiltinMethod.TableGetN, DataValueType.Integer},
                {LuaBuiltinMethod.TableConcat, DataValueType.String}
            };
        
        public LuaBuiltinMethod Method { get; set; }

        public IList<ExpressionNode> Arguments { get; set; }

        public CallBuiltinLuaMethodNode()
            : base(RedILNodeType.CallLuaMethod)
        {
            Arguments = new List<ExpressionNode>();
        }

        public CallBuiltinLuaMethodNode(
            LuaBuiltinMethod method,
            IList<ExpressionNode> arguments)
            : base(RedILNodeType.CallLuaMethod, MethodTypeTable[method])
        {
            Method = method;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallBuiltinLuaMethodNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is CallBuiltinLuaMethodNode)) return false;
            var callLuaMethod = (CallBuiltinLuaMethodNode) other;
            return Method == callLuaMethod.Method && Arguments.AllEqual(callLuaMethod.Arguments);
        }

        public override ExpressionNode Simplify() => new CallBuiltinLuaMethodNode(Method, Arguments.Select(arg => arg.Simplify()).ToList());
    }
}