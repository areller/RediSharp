using System.Collections.Generic;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class BlockNode : RedILNode
    {
        public IList<RedILNode> Children { get; set; }

        public IList<RedILNode> PostExecution { get; set; }

        public bool Explicit { get; set; }

        public BlockNode() : base(RedILNodeType.Block)
        {
            Children = new List<RedILNode>();
            PostExecution = new List<RedILNode>();
        }

        public BlockNode(IList<RedILNode> children, bool @explicit = true)
            : base(RedILNodeType.Block)
        {
            Children = children;
            Explicit = @explicit;
            PostExecution = new List<RedILNode>();
        }

        public void Consume(BlockNode anotherBlock)
        {
            foreach (var child in anotherBlock.Children) Children.Add(child);
            foreach (var postExec in anotherBlock.PostExecution) PostExecution.Add(postExec);
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitBlockNode(this, state);
    }
}