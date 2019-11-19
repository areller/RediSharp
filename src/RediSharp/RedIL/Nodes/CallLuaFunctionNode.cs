using System.Collections.Generic;
using System.Linq;
using RediSharp.Lua;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
{
    class CallLuaFunctionNode : ExpressionNode
    {
        public LuaFunction Name { get; set; }

        public IList<ExpressionNode> Arguments { get; set; }

        public CallLuaFunctionNode()
            : base(RedILNodeType.CallLuaFunction)
        {
        }

        public CallLuaFunctionNode(
            LuaFunction name,
            DataValueType type,
            IList<ExpressionNode> arguments)
            : base(RedILNodeType.CallLuaFunction, type)
        {
            Name = name;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallLuaFunctionNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is CallLuaFunctionNode)) return false;
            var callLuaFunction = (CallLuaFunctionNode) other;
            return Name == callLuaFunction.Name && Arguments.AllEqual(callLuaFunction.Arguments);
        }

        public override ExpressionNode Simplify() => new CallLuaFunctionNode(Name, DataType, Arguments.Select(arg => arg.Simplify()).ToList());
    }
}