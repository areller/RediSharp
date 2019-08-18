using RediSharp.RedIL.Enums;
using RediSharp.RedIL.Nodes;
using RediSharp.RedIL.Resolving;
using RediSharp.RedIL.Resolving.Attributes;

namespace RediSharp.RedIL.ExternalResolvers
{
    class NullableValueResolver : RedILMemberResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller)
        {
            return caller;
        }
    }
    
    class NullableHasValueResolver : RedILMemberResolver
    {
        public override ExpressionNode Resolve(Context context, ExpressionNode caller)
        {
            return new BinaryExpressionNode(
                DataValueType.Boolean,
                BinaryExpressionOperator.NotEqual,
                caller,
                new NilNode());
        }
    }
}