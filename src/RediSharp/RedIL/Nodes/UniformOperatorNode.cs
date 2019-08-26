using System.Collections.Generic;
using System.Linq;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
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

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is UniformOperatorNode)) return false;
            var uniform = (UniformOperatorNode) other;
            return Operator == uniform.Operator &&
                   Children.AllEqual(uniform.Children);
        }

        public override ExpressionNode Simplify() =>
            new UniformOperatorNode(DataType, Operator, Children.Select(c => c.Simplify()).ToList());
    }
}