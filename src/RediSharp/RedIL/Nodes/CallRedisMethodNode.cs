using System.Collections.Generic;
using System.Linq;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
{
    class CallRedisMethodNode : ExpressionNode
    {
        public ExpressionNode Method { get; set; }

        public ExpressionNode Caller { get; set; }

        public IList<ExpressionNode> Arguments { get; set; }

        public CallRedisMethodNode()
            : base(RedILNodeType.CallRedisMethod)
        {
            Arguments = new List<ExpressionNode>();
        }

        public CallRedisMethodNode(
            string method,
            DataValueType type,
            ExpressionNode caller,
            IList<ExpressionNode> arguments)
            : this((ConstantValueNode) method, type, caller, arguments)
        {
        }

        public CallRedisMethodNode(
            ExpressionNode method,
            DataValueType type,
            ExpressionNode caller,
            IList<ExpressionNode> arguments)
            : base(RedILNodeType.CallRedisMethod, type)
        {
            Method = method;
            Caller = caller;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallRedisMethodNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is CallRedisMethodNode)) return false;
            var callMethod = (CallRedisMethodNode) other;
            return Method.EqualOrNull(callMethod.Method) &&
                   Caller.EqualOrNull(callMethod.Caller) &&
                   Arguments.AllEqual(callMethod.Arguments);
        }

        public override ExpressionNode Simplify() => new CallRedisMethodNode(Method, DataType, Caller.Simplify(),
            Arguments.Select(arg => arg.Simplify()).ToList());
    }
}