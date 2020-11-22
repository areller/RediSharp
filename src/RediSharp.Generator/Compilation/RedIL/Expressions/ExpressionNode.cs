namespace RediSharp.Generator.Compilation.RedIL.Expressions
{
    abstract class ExpressionNode : Node
    {
        public DataValueType DataValueType { get; }

        protected ExpressionNode(DataValueType dataValueType, Node? parent)
            : base(parent)
        {
            DataValueType = dataValueType;
        }
    }
}