using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class BinaryExpressionNode : ExpressionNode
    {
        public BinaryExpressionOperator Operator { get; set; }

        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public BinaryExpressionNode() : base(RedILNodeType.BinaryExpression) { }

        public BinaryExpressionNode(
            DataValueType dataType,
            BinaryExpressionOperator op,
            ExpressionNode left,
            ExpressionNode right)
            : base(RedILNodeType.BinaryExpression, dataType)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitBinaryExpressionNode(this, state);
    }
}