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

        public ExpressionNode[] Arguments { get; set; }

        public CallRedisMethodNode() : base(RedILNodeType.CallRedisMethod) { }

        public CallRedisMethodNode(
            RedisCommand method,
            ExpressionNode[] arguments)
            : base(RedILNodeType.CallRedisMethod, CommandTypeTable[method])
        {
            Method = method;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallRedisMethodNode(this, state);
    }
}