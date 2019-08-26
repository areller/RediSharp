using System;
using ICSharpCode.Decompiler.Semantics;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
{
    class UnaryExpressionNode : ExpressionNode
    {
        public UnaryExpressionOperator Operator { get; set; }

        public ExpressionNode Operand { get; set; }

        public UnaryExpressionNode() : base(RedILNodeType.UnaryExpression) { }

        public UnaryExpressionNode(
            UnaryExpressionOperator op,
            ExpressionNode operand)
            : base(RedILNodeType.UnaryExpression)
        {
            Operator = op;
            Operand = operand;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitUnaryExpressionNode(this, state);
        
        #region Static

        public static ExpressionNode Create(UnaryExpressionOperator op, ExpressionNode node)
        {
            var unary = new UnaryExpressionNode(op, node);
            return unary.Simplify();
        }

        #endregion

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is UnaryExpressionNode)) return false;
            var unary = (UnaryExpressionNode) other;
            return Operator == unary.Operator &&
                   Operand.EqualOrNull(unary.Operand);
        }

        public override ExpressionNode Simplify()
        {
            if (Operator == UnaryExpressionOperator.Minus)
            {
                if (Operand.Type == RedILNodeType.Constant)
                {
                    var constant = (ConstantValueNode) Operand;
                    if (constant.DataType == DataValueType.Integer)
                    {
                        return new ConstantValueNode(constant.DataType, -Convert.ToInt64(constant.Value));
                    }
                    else if (constant.DataType == DataValueType.Float)
                    {
                        return new ConstantValueNode(constant.DataType, -Convert.ToDouble(constant.Value));
                    }
                }
                else if (Operand.Type == RedILNodeType.UnaryExpression)
                {
                    var unary = (UnaryExpressionNode) Operand;
                    if (unary.Operator == UnaryExpressionOperator.Minus)
                        return unary.Operand;
                }
            }
            else if (Operator == UnaryExpressionOperator.Not)
            {
                if (Operand.Type == RedILNodeType.Constant)
                {
                    var constant = (ConstantValueNode) Operand;
                    if (constant.DataType == DataValueType.Boolean)
                    {
                        return new ConstantValueNode(Operand.DataType, !((bool) constant.Value));
                    }
                }
                else if (Operand.Type == RedILNodeType.UnaryExpression)
                {
                    var unary = (UnaryExpressionNode) Operand;
                    if (unary.Operator == UnaryExpressionOperator.Not)
                        return unary.Operand;
                }
                else if (Operand.Type == RedILNodeType.BinaryExpression)
                {
                    var binary = (BinaryExpressionNode) Operand;
                    if (binary.Operator == BinaryExpressionOperator.Equal)
                        return new BinaryExpressionNode(binary.DataType, BinaryExpressionOperator.NotEqual, binary.Left,
                            binary.Right);
                    else if (binary.Operator == BinaryExpressionOperator.NotEqual)
                        return new BinaryExpressionNode(binary.DataType, BinaryExpressionOperator.Equal, binary.Left,
                            binary.Right);
                    else if (binary.Operator == BinaryExpressionOperator.Less)
                        return new BinaryExpressionNode(binary.DataType, BinaryExpressionOperator.GreaterEqual,
                            binary.Left, binary.Right);
                    else if (binary.Operator == BinaryExpressionOperator.Greater)
                        return new BinaryExpressionNode(binary.DataType, BinaryExpressionOperator.LessEqual,
                            binary.Left, binary.Right);
                    else if (binary.Operator == BinaryExpressionOperator.LessEqual)
                        return new BinaryExpressionNode(binary.DataType, BinaryExpressionOperator.Greater, binary.Left,
                            binary.Right);
                    else if (binary.Operator == BinaryExpressionOperator.GreaterEqual)
                        return new BinaryExpressionNode(binary.DataType, BinaryExpressionOperator.Less, binary.Left,
                            binary.Right);
                }
            }

            return this;
        }
    }
}