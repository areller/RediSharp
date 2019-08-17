using System.Collections.Generic;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class CallLuaMethodNode : ExpressionNode
    {
        private static readonly Dictionary<LuaMethod, DataValueType> MethodTypeTable
            = new Dictionary<LuaMethod, DataValueType>()
            {
                { LuaMethod.StringToLower, DataValueType.String },
                { LuaMethod.TableUnpack, DataValueType.Array }
            };
        
        public LuaMethod Method { get; set; }

        public IList<ExpressionNode> Arguments { get; set; }

        public CallLuaMethodNode()
            : base(RedILNodeType.CallLuaMethod)
        {
            Arguments = new List<ExpressionNode>();
        }

        public CallLuaMethodNode(
            LuaMethod method,
            IList<ExpressionNode> arguments)
            : base(RedILNodeType.CallLuaMethod, MethodTypeTable[method])
        {
            Method = method;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallLuaMethodNode(this, state);
    }
}