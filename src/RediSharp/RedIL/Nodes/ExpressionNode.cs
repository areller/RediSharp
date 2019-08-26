using System;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    abstract class ExpressionNode : RedILNode, IEquatable<ExpressionNode>
    {
        public DataValueType DataType { get; set; }

        protected ExpressionNode(RedILNodeType nodeType) : base(nodeType)
        {
        }

        public ExpressionNode(
            RedILNodeType nodeType,
            DataValueType dataType)
            : base(nodeType)
        {
            DataType = dataType;
        }
        
        #region Defaults
        
        public static readonly ExpressionNode Nil = new NilNode();
        
        public static readonly ExpressionNode True = new ConstantValueNode(DataValueType.Boolean, true);
        
        public static readonly ExpressionNode False = new ConstantValueNode(DataValueType.Boolean, false);

        public static readonly ExpressionNode Zero = new ConstantValueNode(DataValueType.Integer, 0);
        
        public static readonly ExpressionNode One = new ConstantValueNode(DataValueType.Integer, 1);
        
        #endregion

        #region Operators

        public static ExpressionNode operator !(ExpressionNode node) =>
            UnaryExpressionNode.Create(UnaryExpressionOperator.Not, node);

        public static ExpressionNode operator -(ExpressionNode node) =>
            UnaryExpressionNode.Create(UnaryExpressionOperator.Minus, node);

        public static ExpressionNode operator +(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(
                nodeA.DataType == DataValueType.String || nodeB.DataType == DataValueType.String
                    ? BinaryExpressionOperator.StringConcat
                    : BinaryExpressionOperator.Add, nodeA, nodeB);

        public static ExpressionNode operator -(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.Subtract, nodeA, nodeB);

        public static ExpressionNode operator *(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.Multiply, nodeA, nodeB);

        public static ExpressionNode operator /(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.Divide, nodeA, nodeB);

        public static ExpressionNode operator %(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.Modulus, nodeA, nodeB);

        public static ExpressionNode operator ==(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.Equal, nodeA, nodeB);

        public static ExpressionNode operator !=(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.NotEqual, nodeA, nodeB);

        public static ExpressionNode operator <(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.Less, nodeA, nodeB);

        public static ExpressionNode operator >(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.Greater, nodeA, nodeB);

        public static ExpressionNode operator <=(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.LessEqual, nodeA, nodeB);

        public static ExpressionNode operator >=(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.GreaterEqual, nodeA, nodeB);

        public static ExpressionNode operator |(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.Or, nodeA, nodeB);

        public static ExpressionNode operator &(ExpressionNode nodeA, ExpressionNode nodeB) =>
            BinaryExpressionNode.Create(BinaryExpressionOperator.And, nodeA, nodeB);

        public static bool operator true(ExpressionNode node) => false;

        public static bool operator false(ExpressionNode node) => false;

        #endregion

        public abstract bool Equals(ExpressionNode other);

        public abstract ExpressionNode Simplify();
    }
}