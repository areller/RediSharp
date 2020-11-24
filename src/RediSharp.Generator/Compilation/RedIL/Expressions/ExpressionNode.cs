namespace RediSharp.Generator.Compilation.RedIL.Expressions
{
    abstract class ExpressionNode : Node
    {
        public DataValueType DataValueType { get; }

        protected ExpressionNode(DataValueType dataValueType)
        {
            DataValueType = dataValueType;
        }
    }
}