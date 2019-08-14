using System.Collections.Generic;
using RedSharper.Enums;
using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL.Nodes
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

        public ExpressionNode[] Arguments { get; set; }

        public CallLuaMethodNode()
            : base(RedILNodeType.CallLuaMethod)
        {
        }

        public CallLuaMethodNode(
            LuaMethod method,
            ExpressionNode[] arguments)
            : base(RedILNodeType.CallLuaMethod, MethodTypeTable[method])
        {
            Method = method;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallLuaMethodNode(this, state);
    }
}