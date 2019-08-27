using System.Collections.Generic;
using System.Linq;
using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Extensions
{
    static class ExpressionExtensions
    {
        private static IList<ExpressionNode> ZeroArguments = new ExpressionNode[0];
        
        public static ExpressionNode At(this IList<ExpressionNode> arguments, int index) =>
            At(arguments, index, new NilNode());
        
        public static ExpressionNode At(this IList<ExpressionNode> arguments, int index, ExpressionNode defaultNode)
        {
            if (index >= arguments.Count)
            {
                return defaultNode;
            }

            return arguments[index];
        }

        public static bool AllEqual(this IList<ExpressionNode> arguments, IList<ExpressionNode> otherArguments)
        {
            if (arguments is null && otherArguments is null) return true;
            return arguments?.SequenceEqual(otherArguments ?? ZeroArguments) ?? false;
        }

        public static bool EqualOrNull(this ExpressionNode node, ExpressionNode other)
        {
            if (node is null && other is null) return true;
            return node?.Equals(other) ?? false;
        }
        
        #region Expression Builders

        public static ExpressionNode IsNil(this ExpressionNode node)
        {
            return node == ExpressionNode.Nil;
        }

        public static ExpressionNode IsNilOrEmpty(this ExpressionNode node)
        {
            return node == ExpressionNode.Nil || node == ExpressionNode.Empty;
        }

        public static ExpressionNode IsNilOrZero(this ExpressionNode node)
        {
            return node == ExpressionNode.Nil || node == ExpressionNode.Zero;
        }
        
        #endregion
    }
}