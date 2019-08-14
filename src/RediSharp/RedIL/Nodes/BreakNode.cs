using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
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