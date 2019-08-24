using System.Collections.Generic;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class CallLuaFunctionNode : ExpressionNode
    {
        public string Name { get; set; }

        public IList<ExpressionNode> Arguments { get; set; }

        public CallLuaFunctionNode()
            : base(RedILNodeType.CallLuaFunction)
        {
        }

        public CallLuaFunctionNode(
            string name,
            DataValueType type,
            IList<ExpressionNode> arguments)
            : base(RedILNodeType.CallLuaFunction, type)
        {
            Name = name;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallLuaFunctionNode(this, state);
    }
}