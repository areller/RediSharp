using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.RedIL.ExternalResolvers
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