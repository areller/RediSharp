using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Extensions
{
    static class OperatorExtensions
    {
        public static bool IsArithmetic(this BinaryExpressionOperator op)
        {
            return op == BinaryExpressionOperator.Add || op == BinaryExpressionOperator.Subtract ||
                   op == BinaryExpressionOperator.Multiply || op == BinaryExpressionOperator.Divide ||
                   op == BinaryExpressionOperator.Modulus;
        }

        public static bool IsRelational(this BinaryExpressionOperator op)
        {
            return op == BinaryExpressionOperator.Equal || op == BinaryExpressionOperator.NotEqual ||
                   op == BinaryExpressionOperator.Less || op == BinaryExpressionOperator.LessEqual ||
                   op == BinaryExpressionOperator.Greater || op == BinaryExpressionOperator.GreaterEqual;
        }

        public static bool IsLogical(this BinaryExpressionOperator op)
        {
            return op == BinaryExpressionOperator.And || op == BinaryExpressionOperator.Or;
        }
    }
}