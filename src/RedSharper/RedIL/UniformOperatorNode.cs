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
            : base(RedILNodeType.UniformExpression, op)
        {
            Operator = op;
            Children = children;
        }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}