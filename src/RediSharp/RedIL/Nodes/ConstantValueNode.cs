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
    }
}