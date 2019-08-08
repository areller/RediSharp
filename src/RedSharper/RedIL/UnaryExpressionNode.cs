using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class UnaryExpressionNode : ExpressionNode
    {
        public UnaryExpressionOperator Operator { get; set; }

        public ExpressionNode Operand { get; set; }

        public UnaryExpressionNode() : base(RedILNodeType.UnaryExpression) { }

        public UnaryExpressionNode(
            UnaryExpressionOperator op,
            ExpressionNode operand)
            : base(RedILNodeType.UnaryExpression)
        {
            Operator = op;
            Operand = operand;
        }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}