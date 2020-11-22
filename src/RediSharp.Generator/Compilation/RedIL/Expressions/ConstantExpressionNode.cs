namespace RediSharp.Generator.Compilation.RedIL.Expressions
{
    sealed class ConstantExpressionNode : ExpressionNode
    {
        public object Value { get; }

        public ConstantExpressionNode(object value, DataValueType dataValueType, Node? parent)
            : base(dataValueType, parent)
        {
            Value = value;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state) => visitor.VisitConstantExpressionNode(this, state);
    }
}