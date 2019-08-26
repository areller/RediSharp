using System;
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
        
        #region Conversions
        
        public static implicit operator ConstantValueNode(int value) => new ConstantValueNode(DataValueType.Integer, value);
        
        public static implicit operator ConstantValueNode(double value) => new ConstantValueNode(DataValueType.Float, value);
        
        public static implicit operator ConstantValueNode(bool value) => new ConstantValueNode(DataValueType.Boolean, value);
        
        public static implicit operator ConstantValueNode(string value) => new ConstantValueNode(DataValueType.String, value);
        
        #endregion

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is ConstantValueNode)) return false;
            if (DataType != other.DataType) return false;
            var constant = (ConstantValueNode) other;

            switch (DataType)
            {
                case DataValueType.Integer:
                    return Convert.ToInt64(Value).Equals(Convert.ToInt64(constant.Value));
                case DataValueType.Float:
                    return Convert.ToDouble(Value).Equals(Convert.ToDouble(constant.Value));
                case DataValueType.String:
                case DataValueType.Boolean:
                case DataValueType.Unknown:
                    return Value.Equals(constant.Value);
                default:
                    return false;
            }
        }

        public override ExpressionNode Simplify() => this;
    }
}