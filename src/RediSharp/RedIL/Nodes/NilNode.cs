using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class NilNode : ExpressionNode
    {
        public NilNode()
            : base(RedILNodeType.Nil, DataValueType.Unknown)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitNilNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            return other is NilNode;
        }

        public override ExpressionNode Simplify() => this;
    }
}