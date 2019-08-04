using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class UnaryExpressionNode : RedILNode
    {
        public UnaryExpressionOperator Operator { get; }

        public RedILNode Operand { get; }

        public UnaryExpressionNode(
            UnaryExpressionOperator op,
            RedILNode operand)
            : base(RedILNodeType.UnaryExpression)
        {
            Operator = op;
            Operand = operand;
        }
    }
}