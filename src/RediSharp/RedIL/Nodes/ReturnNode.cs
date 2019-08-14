using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class ReturnNode : RedILNode
    {
        public ExpressionNode Value { get; set; }

        public ReturnNode() : base(RedILNodeType.Return) { }

        public ReturnNode(ExpressionNode value)
            : base(RedILNodeType.Return)
        {
            Value = value;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitReturnNode(this, state);
    }
}