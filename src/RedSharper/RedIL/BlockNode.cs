using System.Collections.Generic;
using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class BlockNode : RedILNode
    {
        public IList<RedILNode> Children { get; set; }

        public BlockNode() : base(RedILNodeType.Block) { }

        public BlockNode(IList<RedILNode> children)
            : base(RedILNodeType.Block)
        {
            Children = children;
        }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            visitor.VisitBlockNode(this, state);
        }
    }
}