using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
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

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitConditionalExpressionNode(this, state);

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is ConditionalExpressionNode)) return false;
            var conditional = (ConditionalExpressionNode) other;
            return Condition.EqualOrNull(conditional.Condition) &&
                   IfYes.EqualOrNull(conditional.IfYes) &&
                   IfNo.EqualOrNull(conditional.IfNo);
        }

        public override ExpressionNode Simplify() => this;
    }
}