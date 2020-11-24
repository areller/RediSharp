namespace RediSharp.Generator.Compilation.RedIL.Expressions
{
    sealed class ConstantExpressionNode : ExpressionNode
    {
        public object Value { get; }

        public ConstantExpressionNode(object value, DataValueType dataValueType)
            : base(dataValueType)
        {
            Value = value;
        }

        public override TReturn? AcceptVisitor<TReturn, TState>(RedILVisitor<TReturn, TState> visitor, TState? state)
            where TReturn : class
            where TState : class
        {
            return visitor.VisitConstantExpressionNode(this, state);
        }
    }
}