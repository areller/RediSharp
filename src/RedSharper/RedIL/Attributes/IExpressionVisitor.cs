namespace RedSharper.RedIL.Attributes
{
    interface IExpressionVisitor
    {
        ExpressionNode Visit(ExpressionNode node);
    }
}