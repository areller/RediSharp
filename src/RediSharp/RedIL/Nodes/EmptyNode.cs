using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    internal class EmptyNode : RedILNode
    {
        public EmptyNode()
            : base(RedILNodeType.Empty)
        { }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitEmptyNode(this, state);
    }
}