using System;
using RedSharper.Contracts.Enums;
using RedSharper.RedIL.Attributes;
using StackExchange.Redis;

namespace RedSharper.Contracts
{
    public class RedSingleResult : RedResult
    {
        private readonly RedisValue _value;
        internal override RedResultType Type { get; }

        public RedSingleResult(RedisValue value, RedResultType? resultType)
        {
            _value = value;
            Type = resultType ?? (value.IsInteger ? RedResultType.Integer : RedResultType.BulkString);
        }

        internal override bool IsNull => _value.IsNull;

        public override string ToString() => _value.ToString();

        #region Implicit Conversions

        public static implicit operator string (RedSingleResult result) => result._value;

        #endregion

        #region Conversions

        [RedILResolve(typeof(SingleResultAsIntResolver))]
        public int? AsInt() => (int)_value;

        [RedILResolve(typeof(SingleResultAsLongResolver))]
        public long? AsLong() => (long)_value;

        [RedILResolve(typeof(SingleResultAsDoubleResolver))]
        public double? AsDouble() => (double)_value;

        #endregion
    }
}