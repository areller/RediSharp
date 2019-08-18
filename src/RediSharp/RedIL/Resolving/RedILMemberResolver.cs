using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving
{
    abstract class RedILMemberResolver
    {
        public abstract ExpressionNode Resolve(Context context, ExpressionNode caller);
    }
}