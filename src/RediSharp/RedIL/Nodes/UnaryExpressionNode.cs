using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
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

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitUnaryExpressionNode(this, state);
        
        #region Static

        public static UnaryExpressionNode Create(UnaryExpressionOperator op, ExpressionNode node)
        {
            //TODO: Handle case where node is constant
            return new UnaryExpressionNode(op, node);
        }

        #endregion
    }
}