using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class ConditionalExpressionNode : ExpressionNode
    {
        private static DataValueType DeduceType(DataValueType left, DataValueType right)
        {
            if (left == DataValueType.Float || right == DataValueType.Float)
            {
                return DataValueType.Float;
            }

            return left;
        }
        
        public ExpressionNode Condition { get; set; }

        public ExpressionNode IfYes { get; set; }

        public ExpressionNode IfNo { get; set; }

        public ConditionalExpressionNode()
            : base(RedILNodeType.Conditional)
        {
        }

        public ConditionalExpressionNode(ExpressionNode condition, ExpressionNode ifYes, ExpressionNode ifNo)
            : base(RedILNodeType.Conditional, DeduceType(ifYes.DataType, ifNo.DataType))
        {
            Condition = condition;
            IfYes = ifYes;
            IfNo = ifNo;
        }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}