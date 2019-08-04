using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class CallRedisMethodNode : RedILNode
    {
        public RedisMethod Method { get; }

        public RedILNode[] Arguments { get; }

        public CallRedisMethodNode(
            RedisMethod method,
            RedILNode[] arguments)
            : base(RedILNodeType.CallRedisMethod)
        {
            Method = method;
            Arguments = arguments;
        }
    }
}