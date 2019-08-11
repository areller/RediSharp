using System.Collections.Generic;
using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class BlockNode : RedILNode
    {
        public IList<RedILNode> Children { get; set; }

        public bool Explicit { get; set; }

        public BlockNode() : base(RedILNodeType.Block)
        {
            Children = new List<RedILNode>();
        }

        public BlockNode(IList<RedILNode> children, bool @explicit = true)
            : base(RedILNodeType.Block)
        {
            Children = children;
            Explicit = @explicit;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitBlockNode(this, state);
    }
}