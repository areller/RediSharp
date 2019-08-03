using RedSharper.Contracts.Enums;
using StackExchange.Redis;

namespace RedSharper.Contracts
{
    public class RedSingleResult : RedResult
    {
        private RedisValue _value;

        public override RedResultType Type { get; }

        public override bool IsNull => _value.IsNull;

        public RedSingleResult(RedisValue value, RedResultType? type)
        {
            Type = type ?? (value.IsInteger ? RedResultType.Integer : RedResultType.BulkString);
        }

        public static implicit operator string (RedSingleResult res) => res.AsString();

        public string AsString() => (string)_value;

        public int AsInteger() => (int)_value;

        public long AsLong() => (long)_value;
    }
}