using RedSharper.Enums;
using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class CallRedisMethodNode : RedILNode
    {
        public RedisCommand Method { get; set; }

        public RedILNode[] Arguments { get; set; }

        public CallRedisMethodNode() : base(RedILNodeType.CallRedisMethod) { }

        public CallRedisMethodNode(
            RedisCommand method,
            RedILNode[] arguments)
            : base(RedILNodeType.CallRedisMethod)
        {
            Method = method;
            Arguments = arguments;
        }

        public override void AcceptVisitor<TState>(IRedILVisitor<TState> visitor, TState state)
        {
            throw new System.NotImplementedException();
        }
    }
}