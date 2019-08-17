using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class RootNode : RedILNode
    {
        public RedILNode Body { get; set; }

        public RootNode(RedILNode body = null)
            : base(RedILNodeType.Root)
        {
            Body = body;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitRootNode(this, state);
    }
}