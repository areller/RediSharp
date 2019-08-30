using RediSharp.RedIL.Nodes;

namespace RediSharp.RedIL.Resolving
{
    abstract class RedILValueResolver
    {
        public abstract ExpressionNode Resolve(Context context, object value);
    }
}