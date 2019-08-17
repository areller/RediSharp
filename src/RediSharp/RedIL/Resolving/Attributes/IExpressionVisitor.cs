using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving.Attributes
{
    interface IExpressionVisitor
    {
        ExpressionNode Visit(ExpressionNode node);
    }
}