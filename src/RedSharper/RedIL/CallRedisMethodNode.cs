using System.Collections.Generic;
using RedSharper.Enums;
using RedSharper.RedIL.Enums;

namespace RedSharper.RedIL
{
    class CallRedisMethodNode : ExpressionNode
    {
        private static readonly Dictionary<RedisCommand, DataValueType> CommandTypeTable
            = new Dictionary<RedisCommand, DataValueType>()
            {
                { RedisCommand.Get, DataValueType.String },
                { RedisCommand.Set, DataValueType.Boolean },
                { RedisCommand.HGet, DataValueType.String },
                { RedisCommand.HMGet, DataValueType.Multi },
                { RedisCommand.HSet, DataValueType.Boolean }
            };

        public RedisCommand Method { get; set; }

        public RedILNode[] Arguments { get; set; }

        public CallRedisMethodNode() : base(RedILNodeType.CallRedisMethod) { }

        public CallRedisMethodNode(
            RedisCommand method,
            RedILNode[] arguments)
            : base(RedILNodeType.CallRedisMethod, CommandTypeTable[method])
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