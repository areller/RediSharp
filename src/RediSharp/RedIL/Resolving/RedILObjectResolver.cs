using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving
{
    abstract class RedILObjectResolver
    {
        public abstract ExpressionNode Resolve(Context context, ExpressionNode[] arguments, ExpressionNode[] elements);
    }
}