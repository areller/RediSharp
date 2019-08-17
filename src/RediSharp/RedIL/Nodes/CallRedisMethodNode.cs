using System.Collections.Generic;
using RediSharp.Enums;
using RediSharp.RedIL.Enums;

namespace RediSharp.RedIL.Nodes
{
    class CallRedisMethodNode : ExpressionNode
    {
        private static readonly Dictionary<RedisCommand, DataValueType> CommandTypeTable
            = new Dictionary<RedisCommand, DataValueType>()
            {
                { RedisCommand.Get, DataValueType.String },
                { RedisCommand.Set, DataValueType.Boolean },
                { RedisCommand.HGet, DataValueType.String },
                { RedisCommand.HMGet, DataValueType.Array },
                { RedisCommand.HSet, DataValueType.Boolean },
                { RedisCommand.HGetAll, DataValueType.Array }
            };

        public RedisCommand Method { get; set; }

        public ExpressionNode Caller { get; set; }

        public IList<ExpressionNode> Arguments { get; set; }

        public CallRedisMethodNode()
            : base(RedILNodeType.CallRedisMethod)
        {
            Arguments = new List<ExpressionNode>();
        }

        public CallRedisMethodNode(
            RedisCommand method,
            ExpressionNode caller,
            IList<ExpressionNode> arguments)
            : base(RedILNodeType.CallRedisMethod, CommandTypeTable[method])
        {
            Method = method;
            Caller = caller;
            Arguments = arguments;
        }

        public override TReturn AcceptVisitor<TReturn, TState>(IRedILVisitor<TReturn, TState> visitor, TState state)
            => visitor.VisitCallRedisMethodNode(this, state);
    }
}