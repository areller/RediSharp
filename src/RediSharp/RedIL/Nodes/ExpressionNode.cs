using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
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