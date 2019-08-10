using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class DoWhileNode : RedILNode
    {
        public ExpressionNode Condition { get; set; }

        public BlockNode Body { get; set; }

        public DoWhileNode()
            : base(RedILNodeType.DoWhile)
        {
        }

        public DoWhileNode(ExpressionNode condition, BlockNode body)
            : base(RedILNodeType.DoWhile)
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