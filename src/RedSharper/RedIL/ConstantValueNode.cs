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

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new NotImplementedException();
        }
    }
}