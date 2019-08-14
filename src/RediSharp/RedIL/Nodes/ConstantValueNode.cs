using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class ConstantValueNode : ExpressionNode
    {
        public object Value { get; set; }

        public ConstantValueNode() : base(RedILNodeType.Constant) { }

        public ConstantValueNode(DataValueType type, object value)
            : base(RedILNodeType.Constant, type)
        {
            Value = value;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitConstantValueNode(this, state);
    }
}