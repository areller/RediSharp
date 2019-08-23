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
        
        #region Operators
        
        public static ExpressionNode operator -(ExpressionNode node) => UnaryExpressionNode.Create(UnaryExpressionOperator.Minus, node);

        public static ExpressionNode operator +(ExpressionNode nodeA, ExpressionNode nodeB) => BinaryExpressionNode.Create(BinaryExpressionOperator.Add, nodeA, nodeB);
        
        public static ExpressionNode operator -(ExpressionNode nodeA, ExpressionNode nodeB) => BinaryExpressionNode.Create(BinaryExpressionOperator.Subtract, nodeA, nodeB);
        
        public static ExpressionNode operator *(ExpressionNode nodeA, ExpressionNode nodeB) => BinaryExpressionNode.Create(BinaryExpressionOperator.Multiply, nodeA, nodeB);
        
        public static ExpressionNode operator /(ExpressionNode nodeA, ExpressionNode nodeB) => BinaryExpressionNode.Create(BinaryExpressionOperator.Divide, nodeA, nodeB);
        
        #endregion
    }
}