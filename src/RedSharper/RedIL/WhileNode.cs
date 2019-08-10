using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
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

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}