using System.Collections.Generic;
using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class UniformOperatorNode : ExpressionNode
    {
        public BinaryExpressionOperator Operator { get; set; }

        public IList<ExpressionNode> Children { get; set; }

        public UniformOperatorNode()
            : base(RedILNodeType.UniformExpression)
        {
            Children = new List<ExpressionNode>();
        }

        public UniformOperatorNode(
            DataValueType dataType,
            BinaryExpressionOperator op,
            IList<ExpressionNode> children)
            : base(RedILNodeType.UniformExpression, dataType)
        {
            Operator = op;
            Children = children;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitUniformOperatorNode(this, state);
    }
}