using System.Collections.Generic;
using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class BlockNode : RedILNode
    {
        public IList<RedILNode> Children { get; }

        public BlockNode(IList<RedILNode> children)
            : base(RedILNodeType.Block)
        {
            Children = children;
        }
    }
}