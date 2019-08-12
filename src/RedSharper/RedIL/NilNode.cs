using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class NilNode : ExpressionNode
    {
        public NilNode()
            : base(RedILNodeType.Nil, DataValueType.Unknown)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitNilNode(this, state);
    }
}