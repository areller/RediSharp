using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL.Nodes
{
    class WhileNode : RedILNode
    {
        public ExpressionNode Condition { get; set; }

        public BlockNode Body { get; set; }

        public WhileNode()
            : base(RedILNodeType.While)
        {
        }

        public WhileNode(ExpressionNode condition, BlockNode body)
            : base(RedILNodeType.While)
        {
            Condition = condition;
            Body = body;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitWhileNode(this, state);
    }
}