using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL.Nodes
{
    class BreakNode : RedILNode
    {
        public BreakNode()
            : base(RedILNodeType.Break)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitBreakNode(this, state);
    }
}