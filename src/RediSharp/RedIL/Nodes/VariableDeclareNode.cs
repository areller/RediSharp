using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class VariableDeclareNode : RedILNode
    {
        public ExpressionNode Name { get; set; }

        public ExpressionNode Value { get; set; }

        public VariableDeclareNode() : base(RedILNodeType.VariableDeclaration) { }

        public VariableDeclareNode(
            string name,
            ExpressionNode value)
            : this((ConstantValueNode) name, value)
        {
        }

        public VariableDeclareNode(
            ExpressionNode name,
            ExpressionNode value)
            : base(RedILNodeType.VariableDeclaration)
        {
            Name = name;
            Value = value;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitVariableDeclareNode(this, state);
    }
}