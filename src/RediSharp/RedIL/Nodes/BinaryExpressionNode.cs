using System;
using System.Diagnostics.Contracts;
using System.Transactions;
using ICSharpCode.Decompiler.IL;
using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Extensions;

namespace RediSharp.RedIL.Nodes
{
    class BinaryExpressionNode : ExpressionNode
    {
        public BinaryExpressionOperator Operator { get; set; }

        public ExpressionNode Left { get; set; }

        public ExpressionNode Right { get; set; }

        public BinaryExpressionNode() : base(RedILNodeType.BinaryExpression) { }

        public BinaryExpressionNode(
            DataValueType dataType,
            BinaryExpressionOperator op,
            ExpressionNode left,
            ExpressionNode right)
            : base(RedILNodeType.BinaryExpression, dataType)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitBinaryExpressionNode(this, state);
        
        #region Static

        public static ExpressionNode Create(BinaryExpressionOperator op, ExpressionNode left, ExpressionNode right)
        {
            var type = DecideValueType(op, left.DataType, right.DataType);
            var node = new BinaryExpressionNode(type, op, left, right);
            return node.Simplify();
        }

        private static DataValueType DecideValueType(BinaryExpressionOperator op, DataValueType left, DataValueType right)
        {
            if (op.IsArithmetic())
            {
                if (left == DataValueType.Integer && right == DataValueType.Integer)
                {
                    return DataValueType.Integer;
                }
                else if (left == DataValueType.Boolean && right == DataValueType.Boolean)
                {
                    return DataValueType.Boolean;
                }
                else if (left == DataValueType.Float || right == DataValueType.Float)
                {
                    return DataValueType.Float;
                }
                else if (left == DataValueType.String || right == DataValueType.String)
                {
                    return DataValueType.String;
                }
            }
            else if (op.IsLogical() || op.IsRelational())
            {
                return DataValueType.Boolean;
            }

            return DataValueType.Unknown;
        }

        #endregion

        public override bool Equals(ExpressionNode other)
        {
            if (!(other is BinaryExpressionNode)) return false;
            var binary = (BinaryExpressionNode) other;
            return Operator == binary.Operator &&
                   Left.Equals(binary.Left) &&
                   Right.Equals(binary.Right);
        }

        public override ExpressionNode Simplify()
        {
            var areEqual = Left.EqualOrNull(Right);
            switch (Operator)
            {
                case BinaryExpressionOperator.StringConcat:
                    return SimplifyStringConcat(Left, Right);
                case BinaryExpressionOperator.Add:
                case BinaryExpressionOperator.Divide:
                case BinaryExpressionOperator.Multiply:
                case BinaryExpressionOperator.Modulus:
                    return SimplifyArithmatic(Operator, Left, Right);
                case BinaryExpressionOperator.Subtract:
                    if (areEqual) return Zero;
                    return SimplifyArithmatic(Operator, Left, Right);
                case BinaryExpressionOperator.Equal:
                    if (areEqual) return True;
                    if (Left.Type == RedILNodeType.Nil) return NotNil(Right) ? False : this;
                    if (Right.Type == RedILNodeType.Nil) return NotNil(Left) ? False : this;
                    if (NotEqual(Left, Right)) return False;
                    return this;
                case BinaryExpressionOperator.NotEqual:
                    if (areEqual) return False;
                    if (Left.Type == RedILNodeType.Nil) return NotNil(Right) ? True : this;
                    if (Right.Type == RedILNodeType.Nil) return NotNil(Left) ? True : this;
                    if (NotEqual(Left, Right)) return True;
                    return this;
                case BinaryExpressionOperator.Or:
                case BinaryExpressionOperator.And:
                    if (areEqual) return Left;
                    return SimplifyBoolean(Operator, Left, Right);
                case BinaryExpressionOperator.Less:
                    if (areEqual) return False;
                    return SimplifyArithmatic(Operator, Left, Right);
                case BinaryExpressionOperator.Greater:
                    if (areEqual) return False;
                    return SimplifyArithmatic(Operator, Left, Right);
                case BinaryExpressionOperator.LessEqual:
                    if (areEqual) return True;
                    return SimplifyArithmatic(Operator, Left, Right);
                case BinaryExpressionOperator.GreaterEqual:
                    if (areEqual) return True;
                    return SimplifyArithmatic(Operator, Left, Right);
                default:
                    return this;
            }
        }

        private ExpressionNode SimplifyStringConcat(ExpressionNode left, ExpressionNode right)
        {
            if (left.Type == RedILNodeType.Constant && right.Type == RedILNodeType.Constant)
            {
                return new ConstantValueNode(DataValueType.String,
                    ((ConstantValueNode) left).ToString() + ((ConstantValueNode) right).ToString());
            }

            return this;
        }

        private ExpressionNode SimplifyArithmatic(BinaryExpressionOperator op, ExpressionNode left,
            ExpressionNode right)
        {
            if (left.Type == RedILNodeType.Constant && right.Type == RedILNodeType.Constant)
            {
                var leftC = (ConstantValueNode) left;
                var rightC = (ConstantValueNode) right;
                var isInteger = left.DataType == DataValueType.Integer && right.DataType == DataValueType.Integer;
                var dataType = isInteger ? DataValueType.Integer : DataValueType.Float;
                var leftVal = isInteger ? Convert.ToInt64(leftC.Value) : Convert.ToDouble(leftC.Value);
                var rightVal = isInteger ? Convert.ToInt64(rightC.Value) : Convert.ToDouble(rightC.Value);
                switch (Operator)
                {
                    case BinaryExpressionOperator.Add:
                        return new ConstantValueNode(dataType, leftVal + rightVal);
                    case BinaryExpressionOperator.Subtract:
                        return new ConstantValueNode(dataType, leftVal - rightVal);
                    case BinaryExpressionOperator.Multiply:
                        return new ConstantValueNode(dataType, leftVal * rightVal);
                    case BinaryExpressionOperator.Divide:
                        return new ConstantValueNode(dataType, leftVal / rightVal);
                    case BinaryExpressionOperator.Modulus:
                        return new ConstantValueNode(dataType, leftVal %  rightVal);
                    case BinaryExpressionOperator.Less:
                        return new ConstantValueNode(DataValueType.Boolean, leftVal < rightVal);
                    case BinaryExpressionOperator.Greater:
                        return new ConstantValueNode(DataValueType.Boolean, leftVal > rightVal);
                    case BinaryExpressionOperator.LessEqual:
                        return new ConstantValueNode(DataValueType.Boolean, leftVal <= rightVal);
                    case BinaryExpressionOperator.GreaterEqual:
                        return new ConstantValueNode(DataValueType.Boolean, leftVal >= rightVal);
                }
            }

            return this;
        }

        private ExpressionNode SimplifyBoolean(BinaryExpressionOperator op, ExpressionNode left, ExpressionNode right)
        {
            if (left.Type == RedILNodeType.Constant || right.Type == RedILNodeType.Constant)
            {
                var constant = left.Type == RedILNodeType.Constant
                    ? (ConstantValueNode) left
                    : (ConstantValueNode) right;
                var other = left.Type == RedILNodeType.Constant ? right : left;
                switch (op)
                {
                    case BinaryExpressionOperator.And:
                        return constant.Value.Equals(true) ? other : False;
                    case BinaryExpressionOperator.Or:
                        return constant.Value.Equals(true) ? True : other;
                }
            }

            return this;
        }

        private bool NotNil(ExpressionNode node) => node.Type == RedILNodeType.Constant ||
                                                    node.Type == RedILNodeType.ArrayTableDefinition ||
                                                    node.Type == RedILNodeType.DictionaryTableDefinition;

        private bool NotEqual(ExpressionNode left, ExpressionNode right)
        {
            if (left.Type == RedILNodeType.Constant && right.Type == RedILNodeType.Constant)
            {
                var leftC = (ConstantValueNode) left;
                var rightC = (ConstantValueNode) right;
                return !leftC.EqualOrNull(rightC);
            }

            return false;
        }
    }
}