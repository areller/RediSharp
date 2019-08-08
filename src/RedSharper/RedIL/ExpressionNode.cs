using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    abstract class ExpressionNode : RedILNode
    {
        public DataValueType DataType { get; set; }

        protected ExpressionNode(RedILNodeType nodeType) : base(nodeType) { }

        public ExpressionNode(
            RedILNodeType nodeType,
            DataValueType dataType)
            : base(nodeType)
        {
            DataType = dataType;
        }
    }
}