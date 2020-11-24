using System.Collections.Generic;

namespace RediSharp.Generator.Compilation.RedIL
{
    sealed class BlockNode : Node
    {
        public IList<Node> Children { get; }

        public BlockNode(IList<Node> children)
        { 
            Children = children;
        }

        public override TReturn? AcceptVisitor<TReturn, TState>(RedILVisitor<TReturn, TState> visitor, TState? state)
            where TReturn : class
            where TState : class
        {
            return visitor.VisitBlockNode(this, state);
        }
    }
}