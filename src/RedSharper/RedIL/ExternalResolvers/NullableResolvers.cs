using RedSharper.RedIL.Attributes;
using RedSharper.RedIL.Enums;
using RedSharper.RedIL.Nodes;

namespace RedSharper.RedIL.ExternalResolvers
{
    class NullableValueResolver : RedILResolver
    {
        public override ExpressionNode Resolve(IExpressionVisitor visitor, ExpressionNode caller, ExpressionNode[] arguments)
        {
            return caller;
        }
    }
    
    class NullableHasValueResolver : RedILResolver
    {
        public override ExpressionNode Resolve(IExpressionVisitor visitor, ExpressionNode caller, ExpressionNode[] arguments)
        {
            return new BinaryExpressionNode(
                DataValueType.Boolean,
                BinaryExpressionOperator.NotEqual,
                caller,
                new NilNode());
        }
    }
}