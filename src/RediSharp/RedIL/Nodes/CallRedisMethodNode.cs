using System.Collections.Generic;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class CallRedisMethodNode : ExpressionNode
    {
        public string Method { get; set; }

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
            : base(RedILNodeType.CallRedisMethod, type)
        {
            Method = method;
            Caller = caller;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallRedisMethodNode(this, state);
    }
}