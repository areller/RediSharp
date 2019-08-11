using System;
using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
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