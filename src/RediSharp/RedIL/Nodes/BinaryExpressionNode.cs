using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
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
        
        #region Static

        public static ExpressionNode Create(BinaryExpressionOperator op, ExpressionNode left, ExpressionNode right)
        {
            var type = DecideValueType(left.DataType, right.DataType);
            //TODO: Handle case where both are constant
            return new BinaryExpressionNode(type, op, left, right);
        }

        private static DataValueType DecideValueType(DataValueType left, DataValueType right)
        {
            if (left == DataValueType.Integer && right == DataValueType.Integer)
            {
                return DataValueType.Integer;
            }
            else if (left == DataValueType.Boolean && right == DataValueType.Boolean)
            {
                return DataValueType.Boolean;
            }
            else if (left == DataValueType.Float || right == DataValueType.Float)
            {
                return DataValueType.Float;
            }
            else if (left == DataValueType.String || right == DataValueType.String)
            {
                return DataValueType.String;
            }

            throw new RedILException($"Unable to deduce combine type of '{left}' and '{right}'");
        }

        #endregion
    }
}