using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Attributes
{
    interface IExpressionVisitor
    {
        ExpressionNode Visit(ExpressionNode node);
    }
}