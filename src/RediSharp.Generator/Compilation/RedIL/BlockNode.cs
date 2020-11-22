using System.Collections.Generic;

namespace RediSharp.Generator.Compilation.RedIL
{
    sealed class BlockNode : Node
    {
        public IList<Node> Children { get; }

        public BlockNode(IList<Node> children, Node? parent)
            : base(parent)
        {
            Children = children;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state) => visitor.VisitBlockNode(this, state);
    }
}