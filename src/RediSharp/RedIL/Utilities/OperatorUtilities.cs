using ICSharpCode.Decompiler.CSharp.Syntax;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Utilities
{
    static class OperatorUtilities
    {
        public static UnaryExpressionOperator UnaryOperator(UnaryOperatorType op)
        {
            switch (op)
            {
                case UnaryOperatorType.Not: return UnaryExpressionOperator.Not;
                case UnaryOperatorType.Minus: return UnaryExpressionOperator.Minus;
                default: throw new RedILException($"No equivalent operator for unary operator '{op}'");
            }
        }

        public static bool IsIncrement(UnaryOperatorType op)
        {
            return op == UnaryOperatorType.Decrement ||
                   op == UnaryOperatorType.Increment ||
                   op == UnaryOperatorType.PostIncrement ||
                   op == UnaryOperatorType.PostDecrement;
        }
        
        public static BinaryExpressionOperator BinaryOperator(AssignmentOperatorType op)
        {
            switch (op)
            {
                case AssignmentOperatorType.Add: return BinaryExpressionOperator.Add;
                case AssignmentOperatorType.Subtract: return BinaryExpressionOperator.Subtract;
                case AssignmentOperatorType.Multiply: return BinaryExpressionOperator.Multiply;
                case AssignmentOperatorType.Divide: return BinaryExpressionOperator.Divide;
                case AssignmentOperatorType.Modulus: return BinaryExpressionOperator.Modulus;
                default: throw new RedILException($"No equivalent operator for assigment operator '{op}'");
            }
        }

        public static BinaryExpressionOperator BinaryOperator(BinaryOperatorType op)
        {
            switch (op)
            {
                case BinaryOperatorType.Add: return BinaryExpressionOperator.Add;
                case BinaryOperatorType.Subtract: return BinaryExpressionOperator.Subtract;
                case BinaryOperatorType.Multiply: return BinaryExpressionOperator.Multiply;
                case BinaryOperatorType.Divide: return BinaryExpressionOperator.Divide;
                case BinaryOperatorType.Modulus: return BinaryExpressionOperator.Modulus;
                case BinaryOperatorType.ConditionalAnd: return BinaryExpressionOperator.And;
                case BinaryOperatorType.ConditionalOr: return BinaryExpressionOperator.Or;
                case BinaryOperatorType.Equality: return BinaryExpressionOperator.Equal;
                case BinaryOperatorType.InEquality: return BinaryExpressionOperator.NotEqual;
                case BinaryOperatorType.LessThan: return BinaryExpressionOperator.Less;
                case BinaryOperatorType.GreaterThan: return BinaryExpressionOperator.Greater;
                case BinaryOperatorType.LessThanOrEqual: return BinaryExpressionOperator.LessEqual;
                case BinaryOperatorType.GreaterThanOrEqual: return BinaryExpressionOperator.GreaterEqual;
                case BinaryOperatorType.NullCoalescing: return BinaryExpressionOperator.NullCoalescing;
                default: throw new RedILException($"No equivalent operator for binary operator '{op}'");
            }
        }

        public static bool IsArithmatic(BinaryExpressionOperator op)
        {
            return op == BinaryExpressionOperator.Add ||
                op == BinaryExpressionOperator.Subtract ||
                op == BinaryExpressionOperator.Multiply ||
                op == BinaryExpressionOperator.Divide ||
                op == BinaryExpressionOperator.Modulus;
        }

        public static bool IsBoolean(BinaryExpressionOperator op)
        {
            return op == BinaryExpressionOperator.And ||
                op == BinaryExpressionOperator.Or ||
                op == BinaryExpressionOperator.Equal ||
                op == BinaryExpressionOperator.NotEqual ||
                op == BinaryExpressionOperator.Less ||
                op == BinaryExpressionOperator.Greater ||
                op == BinaryExpressionOperator.LessEqual ||
                op == BinaryExpressionOperator.GreaterEqual;
        }
    }
}