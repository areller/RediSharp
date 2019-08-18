using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving
{
    abstract class RedILMethodResolver
    {
        public abstract ExpressionNode Resolve(Context context, ExpressionNode caller, ExpressionNode[] arguments);
    }
}